from mpl_toolkits.mplot3d import Axes3D # for projection='3d'
import matplotlib.pyplot as plt
import numpy as np
from scipy import ndimage

import cv2

import pprint

def read_image(file_name):
    # Read image
    print('Read image: ' + file_name)
    img = cv2.imread(file_name, cv2.IMREAD_COLOR)
    return img

def preprocess(img):
    # Convert BGR(CV_8UC3) to L*a*b*(CV_32FC3) space
    img_lab = np.float32(cv2.cvtColor(img, cv2.COLOR_BGR2LAB))
    img_lab[:, :, 0] *= (100/255) # de-normalization (0 <= L* <= 100)
    img_lab[:, :, 1:] -= 128      # de-normalization (-128 <= a*, b* <= 128)
    return img_lab

def decode_label(label_encoded):
    label = np.zeros((label_encoded.shape[0], label_encoded.shape[1]), dtype='int32')
    label |= label_encoded[:, :, 2]
    label <<= 8
    label |= label_encoded[:, :, 1]
    label <<= 8
    label |= label_encoded[:, :, 0]
    return label

SUPERPIXEL_WINDOW_THRESHOLD = 128 * 128
PCA_THRESHOLD = 50

def weighted_quantile(data, quantile, weights=None):
    if weights is None:
        return np.quantile(data, quantile)
    else:
        ind = np.argsort(data)
        data_sorted = data[ind]
        weights_sorted = weights[ind]
        return np.interp(quantile, np.cumsum(weights_sorted) / np.sum(weights_sorted), data_sorted)

def pca(img, slic_label, avg, max_level, level, x, y, width, height, out, iX, iY, out_r2):
    # Perform PCA recursively
    if level == max_level - 1:
        # Calculate statistics directly
        labels, weights = np.unique(slic_label[x:x+width, y:y+height], return_counts=True)
        mask    = avg[labels][:, 0] > 1
        labels  = labels [mask]
        weights = weights[mask]
        X       = avg[labels]
        if labels.shape[0] > 0:
            center  = np.average(X, axis=0, weights=weights)
        else:
            center  = np.zeros((3))

    else:
        # Calculate statistics from subwindows
        midX = x + (width  // 2)
        midY = y + (height // 2)

        l1, w1, c1 = pca(img, slic_label, avg, max_level, level+1, x,    y,    width//2,     height//2,     out, 2*iX,   2*iY,   out_r2)
        l2, w2, c2 = pca(img, slic_label, avg, max_level, level+1, midX, y,    (width+1)//2, height//2,     out, 2*iX+1, 2*iY,   out_r2)
        l3, w3, c3 = pca(img, slic_label, avg, max_level, level+1, x,    midY, width//2,     (height+1)//2, out, 2*iX,   2*iY+1, out_r2)
        l4, w4, c4 = pca(img, slic_label, avg, max_level, level+1, midX, midY, (width+1)//2, (height+1)//2, out, 2*iX+1, 2*iY+1, out_r2)

        d1 = dict(zip(list(l1), list(w1)))
        d2 = dict(zip(list(l2), list(w2)))
        d3 = dict(zip(list(l3), list(w3)))
        d4 = dict(zip(list(l4), list(w4)))

        labels  = np.unique(np.concatenate((l1, l2, l3, l4)))
        X       = avg[labels]
        d_W     = { label: d1.get(label, 0) + d2.get(label, 0) + d3.get(label, 0) + d4.get(label, 0) for label in set(d1) | set(d2) | set(d3) | set(d4) }
        weights = np.array([ d_W[label] for label in sorted(d_W.keys()) ])
        if labels.shape[0] > 0:
            center  = (c1 * np.sum(w1) + c2 * np.sum(w2) + c3 * np.sum(w3) + c4 * np.sum(w4)) / np.sum(weights)
        else:
            center  = np.zeros((3))

    if labels.shape[0] >= PCA_THRESHOLD:
        # Perform PCA via SVD
        X_prime = (X - center) * np.sqrt(weights)[..., np.newaxis]

        U, S, Vt = np.linalg.svd(X_prime)
        fpc = Vt[0]
        if fpc[0] < 0:
            fpc = -fpc

        X_V = np.matmul(X_prime, Vt.transpose())
        TSS = np.sum(np.square(X_prime))
        RSS = np.sum(np.square(X_V[:, 1:]))
        R_squared = 1 - RSS/TSS

        # fpc_distribution = np.matmul(X - center, Vt.transpose())[:, 0]
        # quartiles = weighted_quantile(fpc_distribution, QUARTILES, weights)
        # out   [level][iX, iY, :] = center[np.newaxis, ...].repeat(8, axis=0) + np.matmul(quartiles[..., np.newaxis], fpc[np.newaxis, ...])
        
        out   [level][iX, iY, :] = center[np.newaxis, ...].repeat(2, axis=0) + np.matmul(np.array([-20, 20])[..., np.newaxis], fpc[np.newaxis, ...])
        out_r2[level][iX, iY]    = R_squared

    return labels, weights, center


def pca_octave(img, slic_label):

    slic_label_unique = np.unique(slic_label)
    num_labels        = slic_label_unique.shape[0]

    segment_avg  = np.ndarray((num_labels, 3))
    for channel in range(3):
        segment_avg  [:, channel] = ndimage.mean(img[:, :, channel], labels=slic_label, index=slic_label_unique)

    out    = []
    out_r2 = []
    window_width, window_height, _ = img.shape
    level = 0

    while window_width * window_height >= SUPERPIXEL_WINDOW_THRESHOLD:
        out.   append(np.zeros(((1 << level), (1 << level), 2, 3)))
        out_r2.append(np.zeros(((1 << level), (1 << level))))
        
        window_width  //= 2
        window_height //= 2
        level += 1

    pca(img, slic_label, segment_avg, level, 0, 0, 0, img.shape[0], img.shape[1], out, 0, 0, out_r2)

    return out, out_r2

file_names = [
    # 'jj-1',
    # 'jj-2',
    # 'sc-1',
    # 'sc-2',
    'sc-3',
    # 'sc-4',
    # 'sf-1',
    # 'sf-2',
    # 'sf-3',
    # 'sf-4',
    # 'sf-5',
    # 'sf-6',
    # 'sf-7',
    # 'sf-8',
    # 'sf-9',
    # 'sf-10',
    # 'sf-11',
    # 'sf-12',
    # 'sf-13',
    # 'sf-14',
    # 'ys-4',
]

if __name__ == "__main__": 

    for file_name in file_names:
        img = read_image('images/mask/' + file_name + '-mask.png')
        img_lab = preprocess(img)
        
        label = decode_label(cv2.imread('images/slic/' + file_name + '-slic.png'))
        
        out, r2 = pca_octave(img_lab, label)
        palette = []
        display = []

        for out_level in out:
            out_level[:, :, :, 0 ] = (out_level[:, :, :, 0] * (255.0/100.0)).clip(min=0.0, max=255.0)
            out_level[:, :, :, 1:] = (out_level[:, :, :, 1: ] + 128).clip(min=0.0, max=255.0)
            out_level = np.uint8(out_level)
            palette_level = np.ndarray(out_level.shape, dtype='uint8')
            for i in range(2):
                palette_level[:, :, i, :] = cv2.cvtColor(out_level[:, :, i, :], cv2.COLOR_LAB2BGR)
            palette.append(palette_level)

            display_level = np.ndarray(( out_level.shape[0], out_level.shape[1] * 2, 3 ), dtype='uint8')
            for i in range(2):
                display_level[:, i*out_level.shape[1]:(i+1)*out_level.shape[1], :] = palette_level[:, :, i, :]
            display.append(display_level)

        for display_level in display:
            cv2.imshow('image', display_level)
            cv2.waitKey(0)
        
