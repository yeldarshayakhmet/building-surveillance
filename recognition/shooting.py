import cv2
import os

cam = cv2.VideoCapture(0)

#cam = cv2.VideoCapture(0) #starts to capture video from camera with index of 0
cam.set(3, 640) # set video width
cam.set(4, 480) # set video height

face_detector = cv2.CascadeClassifier('haarcascade_frontalface_default.xml') #sets Cascade Classifier

# For each person enter unique id
face_id = input('\n write id end press enter ==  ')

print("\n  Initializing face capture. Look the camera and wait ...")

count = 0

while(True):

            ret, img = cam.read()
            img = cv2.flip(img, 1) # flip video image vertically
            gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY) #converts frames into gray scale
            faces = face_detector.detectMultiScale(gray, 1.3, 5) #identifies the size of the image or the size of rectangle that covers face

            for (x,y,w,h) in faces:

                    cv2.rectangle(img, (x,y), (x+w,y+h), (255,0,0), 2) #helps to point out the faces with rectangle
                    count += 1

                    # Saves taken image into the datasets folder
                    cv2.imwrite("dataset/User." + str(face_id) + '.' + str(count) + ".jpg", gray[y:y+h,x:x+w])

                    cv2.imshow('image', img) # updates the content of the window with a new image

            k = cv2.waitKey(100) & 0xff # Press 'ESC' for exiting video
            if k == 27:
                    break
            elif count >= 30: # Take 30 face sample and stop video
                    break

print("\n Stopping the program ...")
cam.release() # stops video capturing
cv2.destroyAllWindows() # closes opened windows
