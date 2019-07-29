from mpl_toolkits.mplot3d import Axes3D # for projection='3d'
import matplotlib.pyplot as plt
import numpy as np
from scipy import ndimage

import cv2
import cv2.ximgproc as ximgproc

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

def slic(img, debug_msg=True):
    if debug_msg:
        print('SLIC - Input image with dimension ' + str(img.shape))

    # Preprocessing: Apply Gaussian Blur
    slic_in = cv2.GaussianBlur(img_lab, (3, 3), 0.8)

    # Apply SLIC
    slic = ximgproc.createSuperpixelSLIC(slic_in, ximgproc.SLIC, 16, 10)
    slic.iterate()

    # Retrieve SLIC results
    slic_label = slic.getLabels()
    slic_contour = slic.getLabelContourMask(False)
    slic_num_segments = slic.getNumberOfSuperpixels()

    if debug_msg:
        print('SLIC - # segments = ' + str(slic_num_segments))

    return slic_num_segments, slic_label, slic_contour

def slic_contour(img, slic_contour):
    out = img.copy()
    out[slic_contour != 0, :] = [0, 0, 255]
    return out

def encode_label(label):
    label_encoded = np.zeros((label.shape[0], label.shape[1], 3), dtype='uint8')
    label_encoded[:, :, 0] = label & 0xFF
    label_encoded[:, :, 1] = (label >>  8) & 0xFF
    label_encoded[:, :, 2] = (label >> 16) & 0xFF
    return label_encoded

def decode_label(label_encoded):
    label = np.zeros((label_encoded.shape[0], label_encoded.shape[1]), dtype='int32')
    label |= label_encoded[:, :, 2]
    label <<= 8
    label |= label_encoded[:, :, 1]
    label <<= 8
    label |= label_encoded[:, :, 0]
    return label

def slic_stats(img, slic_label):
    # Statistics
    slic_label_unique, segment_count = np.unique(slic_label, return_counts=True)
    num_labels    = slic_label_unique.shape[0]

    segment_avg   = np.ndarray((num_labels, 3))
    segment_stdev = np.ndarray((num_labels, 3))
    
    for channel in range(3):
        segment_avg  [:, channel] = ndimage.mean              (img[:, :, channel], labels=slic_label, index=slic_label_unique)
        segment_stdev[:, channel] = ndimage.standard_deviation(img[:, :, channel], labels=slic_label, index=slic_label_unique)

    segment_count = segment_count[segment_avg[:, 0] > 1]
    segment_stdev = segment_stdev[segment_avg[:, 0] > 1]
    segment_avg   = segment_avg  [segment_avg[:, 0] > 1]
    num_labels    = segment_avg.shape[0]

    print('SLIC - Stats: # valid segments       = ' + str(num_labels))
    print('SLIC - Stats: AVG(NUM_SEG)           = ' + str(np.average(segment_count)))
    print('SLIC - Stats: STDEV(NUM_SEG)         = ' + str(np.std(segment_count)))
    print('SLIC - Stats: AVG(STDEV_SEG(L*a*b*)) = ' + str(np.average(segment_stdev, axis=0)))

    return num_labels, segment_count, segment_avg, segment_stdev[:, 0]

def weighted_quantile(data, quantile, weights=None):
    if weights is None:
        return np.quantile(data, quantile)
    else:
        ind = np.argsort(data)
        data_sorted = data[ind]
        weights_sorted = weights[ind]
        return np.interp(quantile, np.cumsum(weights_sorted) / np.sum(weights_sorted), data_sorted)

def pca(X, weights=None):
    # Linear Fit (PCA) using SVD
    X_bar   = np.average(X, axis=0, weights=weights)
    if weights is None:
        X_prime = X - X_bar
    else:
        X_prime = (X - X_bar) * np.sqrt(weights)[..., np.newaxis]
    
    U, S, Vt = np.linalg.svd(X_prime)
    FPC = Vt[0]
    X_V = np.matmul(X_prime, Vt.transpose())
    TSS = np.sum(np.square(X_prime))
    RSS = np.sum(np.square(X_V[:, 1:]))
    R_squared = 1 - RSS/TSS

    print('PCA: Center    = ' + str(X_bar))
    print('PCA: Direction = ' + str(FPC))
    print('PCA: R^2       = ' + str(R_squared))

    FPC_distribution = np.matmul(X - X_bar, Vt.transpose())[:, 0]
    quartiles = weighted_quantile(FPC_distribution, [.030, .143, .286, .428, .571, .714, .857, .970], weights)
    FPC_line = X_bar[..., np.newaxis].repeat(8, axis=1) + np.matmul(FPC[..., np.newaxis], quartiles[..., np.newaxis].transpose())

    print(FPC_line.transpose())

    return X_bar, FPC, R_squared, FPC_distribution, FPC_line

def draw_image(img, window_title='Display'):
    cv2.imshow(window_title, img)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

def plot_slic_scatter(slic_num_segments, slic_segment_avg, file_name, line_fit=None):
    # Create Figure
    fig = plt.figure(figsize=(20, 20))
    ax  = fig.add_subplot(111, projection='3d')

    # Generate Scatter Plot Colors
    slic_segment_avg_rgb = np.zeros((1, slic_num_segments, 3))
    slic_segment_avg_rgb[0, :, 0]  = slic_segment_avg[:, 0]  * (255/100)
    slic_segment_avg_rgb[0, :, 1:] = slic_segment_avg[:, 1:] + 128
    slic_segment_avg_rgb = cv2.cvtColor(np.uint8(slic_segment_avg_rgb), cv2.COLOR_LAB2RGB)
    plot_colors = (slic_segment_avg_rgb[0, :, :] / 255).tolist()

    # Scatter Plot
    ax.scatter(
        slic_segment_avg[:, 1].tolist(),
        slic_segment_avg[:, 2].tolist(),
        slic_segment_avg[:, 0].tolist(),
        s=1,
        c=plot_colors,
        marker='o',
        alpha=.15
    )

    # Line Plot
    if line_fit is not None:
        ax.plot(
            line_fit[1, :],
            line_fit[2, :],
            line_fit[0, :],
            's-r',
        )

    ax.set_xlabel('A*')
    ax.set_ylabel('B*')
    ax.set_zlabel('L*')
    ax.set_xlim((-32, 32))
    ax.set_ylim((-32, 32))
    ax.set_zlim((0, 100))

    # Save Figure
    fig.savefig('plots/' + file_name + '-SPXScatter.png')
    plt.close(fig)

def plot_slic_stdev(slic_num_segments, slic_segment_avg, slic_segment_stdev, file_name):
    # Create Figure
    fig = plt.figure(figsize=(20, 20))
    ax  = fig.add_subplot(111)

    # Generate Scatter Plot Colors
    slic_segment_avg_rgb = np.zeros((1, slic_num_segments, 3))
    slic_segment_avg_rgb[0, :, 0]  = slic_segment_avg[:, 0]  * (255/100)
    slic_segment_avg_rgb[0, :, 1:] = slic_segment_avg[:, 1:] + 128
    slic_segment_avg_rgb = cv2.cvtColor(np.uint8(slic_segment_avg_rgb), cv2.COLOR_LAB2RGB)
    plot_colors = (slic_segment_avg_rgb[0, :, :] / 255).tolist()

    # Scatter Plot
    ax.scatter(
        slic_segment_avg[:, 0].tolist(),
        slic_segment_stdev.tolist(),
        s=1,
        c=plot_colors,
        marker='o'
    )

    ax.set_xlabel('L*')
    ax.set_ylabel('STDEV(L*)')

    # Save Figure
    fig.savefig('plots/' + file_name + '-SPXSTDL.png')
    plt.close(fig)

def plot_slic_distribution(distribution, file_name, weights=None):
    # Create Figure
    fig = plt.figure(figsize=(20, 20))
    ax = fig.add_subplot(111)
    
    # Histogram
    ax.hist(distribution, bins=1000, weights=weights)
    
    # Save Figure
    fig.savefig('plots/' + file_name + '-GLOBALL.png')
    plt.close(fig)

file_names = [
    'jj-1',
    # 'jj-2',
    # 'sc-1',
    # 'sc-2',
    # 'sc-3',
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

    # f = open('linreg-PIX.csv', 'w')
    # columns = ['FILE_NAME', 'GLOBAL_PIX CENTER L*', 'GLOBAL_PIX CENTER A*', 'GLOBAL_PIX CENTER B*', 'GLOBAL_PIX FPC L*', 'GLOBAL_PIX FPC A*', 'GLOBAL_PIX FPC B*', 'GLOBAL_PIX R^2']
    # f.write(','.join(columns) + '\n')   

    for file_name in file_names:
        img = read_image('images/mask/' + file_name + '-mask.png')
        img_lab = preprocess(img)

        # num_seg, label, contour = slic(img_lab)
        # cv2.imwrite('images/slic/' + file_name + '.png', encode_label(label))
        
        label = decode_label(cv2.imread('images/slic/' + file_name + '-slic.png'))
        
        # draw_image(slic_contour(img, contour))
        num_labels, count, avg, stdev = slic_stats(img_lab, label)
        center, fpc, r2, distribution, line = pca(avg, weights=count)

        # data = [file_name, center[0], center[1], center[2], fpc[0], fpc[1], fpc[2], r2]
        # f.write(','.join(list(map(str, data))) + '\n')

        # plt.style.use(['dark_background'])
        # plot_slic_scatter(num_labels, avg, file_name, line_fit=line)
    
    # f.close()

