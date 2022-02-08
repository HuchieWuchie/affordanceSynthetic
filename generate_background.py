import numpy as np, random
from PIL import Image, ImageFilter
import sys

image_size = 256
dX, dY = image_size, image_size
xArray = np.linspace(0.0, 1.0, dX).reshape((1, dX, 1))
yArray = np.linspace(0.0, 1.0, dY).reshape((dY, 1, 1))

def randColor():
    return np.array([random.random(), random.random(), random.random()]).reshape((1, 1, 3))
def getX(): return xArray
def getY(): return yArray
def safeDivide(a, b):
    return np.divide(a, np.maximum(b, 0.001))

functions = [(0, randColor),
             (0, getX),
             (0, getY),
             (1, np.sin),
             (1, np.cos),
             (1, np.tan),
             (2, np.add),
             (2, np.subtract),
             (2, np.multiply),
             (2, safeDivide)]

depthMin = random.randint(2, 11)
depthMax = random.randint(10, 21)
#depthMin = 2
#depthMax = 8

def buildImg(depth = 0):
    funcs = [f for f in functions if
                (f[0] > 0 and depth < depthMax) or
                (f[0] == 0 and depth >= depthMin)]
    nArgs, func = random.choice(funcs)
    args = [buildImg(depth + 1) for n in range(nArgs)]
    return func(*args)

success = False
while success is False:
    try:
        #image_path = "output/img.png"
        image_path = "/home/daniel/unity_first_test/Assets/Resources/Backgrounds/img.png"
        img = buildImg()
        # Ensure it has the right dimensions, dX by dY by 3
        img = np.tile(img, (dX / img.shape[0], dY / img.shape[1], 3 / img.shape[2]))
        # Convert to 8-bit, send to PIL and save
        img8Bit = np.uint8(np.rint(img.clip(0.0, 1.0) * 255.0))
        img = Image.fromarray(img8Bit)
        img.save(image_path)
        image_path = "/home/daniel/unity_first_test/Assets/Resources/Backgrounds/img_blurred.png"
        blur = random.randint(0, 100)
        blurred_img = img.filter(ImageFilter.GaussianBlur(radius = 50))
        blurred_img.save(image_path)
        success = True
    except TypeError:
        print("TypeError")
