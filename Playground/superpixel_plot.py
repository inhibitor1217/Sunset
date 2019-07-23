from mpl_toolkits.mplot3d import Axes3D # for projection='3d'
import matplotlib.pyplot as plt
import numpy as np
from scipy import ndimage

import cv2
import cv2.ximgproc as ximgproc
 
# Read image
img = cv2.imread('images/sc-3-ocean.png', cv2.IMREAD_COLOR)
img_lab = np.float32(cv2.cvtColor(img, cv2.COLOR_BGR2LAB))
img_lab[:, :, 0] *= (100/255) # de-normalization (0 <= L* <= 100)
img_lab[:, :, 1:] -= 128      # de-normalization (-128 <= a*, b* <= 128)

# Preprocessing
slic_in = cv2.GaussianBlur(img_lab, (3, 3), 0.8)

# Apply SLIC
slic = ximgproc.createSuperpixelSLIC(slic_in, ximgproc.SLIC, 12, 10)
slic.iterate()

# Retrieve SLIC results
slic_label = slic.getLabels()
slic_contour = slic.getLabelContourMask(False)
slic_num_segments = slic.getNumberOfSuperpixels()

print("# Segments: " + str(slic_num_segments))

# Contour image
# out = img
# out[slic_contour != 0, :] = [0, 0, 255]
# cv2.imshow('contour', out)
# cv2.waitKey(0)
# cv2.destroyAllWindows()

# Statistics
segment_avg   = np.ndarray((slic_num_segments, 3))
segment_stdev = np.ndarray((slic_num_segments, 3))
for channel in range(3):
    segment_avg  [:, channel] = ndimage.mean(img_lab[:, :, channel], labels=slic_label, index=np.arange(slic_num_segments))
    segment_stdev[:, channel] = ndimage.standard_deviation(img_lab[:, :, channel], labels=slic_label, index=np.arange(slic_num_segments))

segment_stdev_L1 = np.linalg.norm(segment_stdev, ord=1, axis=1)
segment_stdev_L1_MAX = np.amax(segment_stdev_L1)

# out = np.ndarray(img.shape)
# for x in range(img.shape[0]):
#     for y in range(img.shape[1]):
#         out[x, y] = segment_stdev_L1[slic_label[x, y]] / segment_stdev_L1_MAX
# cv2.imshow('out', out)
# cv2.waitKey(0)
# cv2.destroyAllWindows(0)

print('Average STDEV along superpixel: ' + str(np.average(segment_stdev, axis=0)))

# Create plot
fig = plt.figure()
ax = fig.add_subplot(121, projection='3d')

segment_avg_rgb = np.zeros((1, slic_num_segments, 3))
segment_avg_rgb[0, :, 0]  = segment_avg[:, 0]  * (255/100)
segment_avg_rgb[0, :, 1:] = segment_avg[:, 1:] + 128
segment_avg_rgb = cv2.cvtColor(np.uint8(segment_avg_rgb), cv2.COLOR_LAB2RGB)
plot_colors = (segment_avg_rgb[0, :, :] / 255).tolist()

ax.scatter(
    segment_avg[:, 1].tolist(),
    segment_avg[:, 2].tolist(),
    segment_avg[:, 0].tolist(),
    s=1,
    c=plot_colors,
    marker='o'
)

# ax.set_xlim((-128, 128))
# ax.set_ylim((-128, 128))
# ax.set_zlim((0, 100))

ax.set_xlabel('A')
ax.set_ylabel('B')
ax.set_zlabel('L')

ax2 = fig.add_subplot(122)
ax2.hist(segment_stdev_L1, 100)

plt.show()
