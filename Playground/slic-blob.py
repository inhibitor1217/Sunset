import numpy as np
import cv2
import cv2.ximgproc as ximgproc
import collections

import matplotlib.pyplot as plt

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

def draw_image(img, window_title='Display'):
    cv2.namedWindow(window_title, cv2.WINDOW_NORMAL)
    cv2.imshow(window_title, img)
    cv2.resizeWindow(window_title, 1024, 1024)
    cv2.waitKey(0)
    cv2.destroyAllWindows()

DX = [1, 0, -1, 0]
DY = [0, 1, 0, -1]

def connected_components(mask, label):
    (width, height) = label.shape
    visit = np.ndarray(label.shape, dtype='uint8')
    valid = lambda x, y: x >= 0 and x < width and y >= 0 and y < height
    sizes = []
    plotX = []
    plotY = []
    for x in range(width):
        for y in range(height):
            if mask[x, y, 0] > 0 and visit[x, y] == 0: # valid pixel
                q = collections.deque()
                cur_label = label[x, y]
                cur_size = 0
                cur_X = 0
                cur_Y = 0
                visit[x, y] = 1
                q.append((x, y))
                while len(q) > 0:
                    (_x, _y) = q.popleft()
                    cur_X += _x
                    cur_Y += _y
                    cur_size += 1
                    for i in range(4):
                        if valid(_x + DX[i], _y + DY[i]) and visit[_x + DX[i], _y + DY[i]] == 0 and label[_x + DX[i], _y + DY[i]] == cur_label:
                            visit[_x + DX[i], _y + DY[i]] = 1
                            q.append((_x + DX[i], _y + DY[i]))
                if cur_size > 30:
                    sizes.append(cur_size)
                    plotX.append(cur_X / cur_size)
                    plotY.append(cur_Y / cur_size)

    return plotX, plotY, sizes

file_names = [
    # 'jj-1',
    # 'jj-2',
    # 'sc-1',
    # 'sc-2',
    # 'sc-3',
    'sc-4',
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

        x, y, sizes = connected_components(img, label)
        max_size = max(sizes)

        fig = plt.figure()
        ax = fig.add_subplot(111)
        ax.scatter(
            y,
            x,
            s=1,
            c=list(map(lambda x:x/max_size, sizes))
        )
        ax.set_xlim((0, 2048))
        ax.set_ylim((0, 2048))

        plt.show()


    
