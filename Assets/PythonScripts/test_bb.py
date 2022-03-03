import cv2
import os

# path
image_path = '/home/daniel/unity_first_test/sample_dataset/images/'
number_of_images = len(os.listdir(image_path)) + 1
textfile_path = '/home/daniel/unity_first_test/sample_dataset/object_labels/'
bounded_image_path = '/home/daniel/unity_first_test/sample_dataset/images_with_bounding_boxes/'
idx = range(1, number_of_images)

for id in idx:
    print("Iteration " + str(id))
    image_path_temp = image_path + str(id) + ".png"
    image = cv2.imread(image_path_temp)
    bounded_image_path_temp = bounded_image_path + str(id) + ".png"

    textfile_path_temp = textfile_path + str(id) + ".txt"
    with open(textfile_path_temp) as f:
        lines = f.readlines()

    for i in range(0, len(lines)):
        bb = lines[i].split();
        for j in range(0, len(bb)):
            bb[j] = int(bb[j])
        image = cv2.rectangle(image, (bb[1], bb[2]), (bb[3], bb[4]), (255, 0, 0), 2)

    cv2.imwrite(bounded_image_path_temp, image)
