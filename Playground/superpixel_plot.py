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

def pca(X):
    # Linear Fit (PCA) using SVD
    X_bar = X.mean(axis=0)
    U, S, Vt = np.linalg.svd(X - X_bar)
    FPC = Vt[0]
    X_V = np.matmul(X - X_bar, Vt.transpose())
    TSS = np.sum(np.square(X - X_bar))
    RSS = np.sum(np.square(X_V[:, 1:]))
    R_squared = 1 - RSS/TSS

    print('PCA: Center    = ' + str(X_bar))
    print('PCA: Direction = ' + str(FPC))
    print('PCA: R^2       = ' + str(R_squared))

    FPC_distribution = X_V[:, 0]

    return X_bar, FPC, FPC_distribution, R_squared

def draw_image(img, window_title='Display'):
    cv2.imshow(window_title, img)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

def slic_plot_stats(slic_num_segments, slic_segment_avg, slic_segment_stdev):
    # Create plot
    fig1 = plt.figure()
    ax1 = fig1.add_subplot(111, projection='3d')
    fig2 = plt.figure()
    ax2 = fig2.add_subplot(111)

    # Process Scatter Plot Colors
    slic_segment_avg_rgb = np.zeros((1, slic_num_segments, 3))
    slic_segment_avg_rgb[0, :, 0]  = slic_segment_avg[:, 0]  * (255/100)
    slic_segment_avg_rgb[0, :, 1:] = slic_segment_avg[:, 1:] + 128
    slic_segment_avg_rgb = cv2.cvtColor(np.uint8(slic_segment_avg_rgb), cv2.COLOR_LAB2RGB)
    plot_colors = (slic_segment_avg_rgb[0, :, :] / 255).tolist()

    # Scatter Plot
    ax1.scatter(
        slic_segment_avg[:, 1].tolist(),
        slic_segment_avg[:, 2].tolist(),
        slic_segment_avg[:, 0].tolist(),
        s=1,
        c=plot_colors,
        marker='o'
    )
    ax1.set_xlabel('A*')
    ax1.set_ylabel('B*')
    ax1.set_zlabel('L*')
    ax1.set_xlim((-32, 32))
    ax1.set_ylim((-32, 32))
    ax1.set_zlim((0, 100))

    ax2.scatter(
        slic_segment_avg[:, 0].tolist(),
        slic_segment_stdev.tolist(),
        s=1,
        c=plot_colors,
        marker='o'
    )
    ax2.set_xlabel('L*')
    ax2.set_ylabel('STDEV(L*)')

    return fig1, fig2

file_names = [
    'jj-1-mask',
    'jj-2-mask',
    'sc-1-mask',
    'sc-2-mask',
    'sc-3-mask',
    'sc-4-mask',
    'sf-1-mask',
    'sf-2-mask',
    'sf-3-mask',
    'sf-4-mask',
    'sf-5-mask',
    'sf-6-mask',
    'sf-7-mask',
    'sf-8-mask',
    'sf-9-mask',
    'sf-10-mask',
    'sf-11-mask',
    'sf-12-mask',
    'sf-13-mask',
    'sf-14-mask',
    'ys-4-mask',
]

if __name__ == "__main__":

    # f = open('linreg.csv', 'w')
    # columns = ['FILE_NAME', 'GLOBAL_EQ CENTER L*', 'GLOBAL_EQ CENTER A*', 'GLOBAL_EQ CENTER B*', 'GLOBAL_EQ FPC L*', 'GLOBAL_EQ FPC A*', 'GLOBAL_EQ FPC B*', 'GLOBAL_EQ R^2']
    # f.write(','.join(columns) + '\n')   

    for file_name in file_names:
        img = read_image('images/mask/' + file_name + '.png')
        img_lab = preprocess(img)
        # num_seg, label, contour = slic(img_lab)
        label = decode_label(cv2.imread('images/slic/' + file_name + '.png'))

        # cv2.imwrite('images/slic/' + file_name + '.png', encode_label(label))
        
        # draw_image(slic_contour(img, contour))
        num_labels, count, avg, stdev = slic_stats(img_lab, label)
        center, fpc, distribution, r2 = pca(avg)

        # data = [file_name, center[0], center[1], center[2], fpc[0], fpc[1], fpc[2], r2]
        # f.write(','.join(list(map(str, data))) + '\n')

        plt.style.use(['dark_background'])
        # fig1, fig2 = slic_plot_stats(num_labels, avg, stdev)
        # fig1.savefig('plots/' + file_name + '-plot1.png')
        # fig2.savefig('plots/' + file_name + '-plot2.png')
        # plt.close(fig1)
        # plt.close(fig2)

        fig = plt.figure()
        ax = fig.add_subplot(111)
        ax.hist(distribution, bins=1000, weights=count)
        fig.savefig('plots/' + file_name + '-plot3.png')
        plt.close(fig)
    
    # f.close()

