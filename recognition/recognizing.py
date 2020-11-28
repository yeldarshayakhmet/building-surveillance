import cv2
import numpy as np
import os
import playsound
import threading
import time
import pygame
import smtplib
import ssl
import remotecv

recognizer = cv2.face.LBPHFaceRecognizer_create()
recognizer.read('trainer/trainer.yml')
cascadePath = "haarcascade_frontalface_default.xml"
faceCascade = cv2.CascadeClassifier(cascadePath);

remotecv.initialize('169.254.246.191', 5800)

font = cv2.FONT_HERSHEY_SIMPLEX

#iniciate id counter
id = 0

# names related to ids:
names = ['None', 'Alisher', 'Yeldar', 'Demo1', 'Demo2', 'Demo3', 'Demo4']

# Initialize and start realtime video capture
cam = cv2.VideoCapture(0)
#cam = cv2.VideoCapture("http://172.20.10.3:8081/")
cam.set(3, 640) # set video widht
cam.set(4, 480) # set video height

# Define min window size to be recognized as a face
minW = 0.1*cam.get(3)
minH = 0.1*cam.get(4)

while True:

    ret, img =cam.read()
    img = cv2.flip(img, 1) # Flip vertically

    gray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)

    faces = faceCascade.detectMultiScale(
        gray,
        scaleFactor = 1.2,
        minNeighbors = 5,
        minSize = (int(minW), int(minH)),
       )

    for(x,y,w,h) in faces:

        facerec = cv2.rectangle(img, (x,y), (x+w,y+h), (0,255,0), 2)

        id, confidence = recognizer.predict(gray[y:y+h,x:x+w]) # predicts the person matching with image
        confidence = 100 - confidence
        
        # Check if confidence is less them 100 ==> "0" is perfect match
        gmail_user = 'yeldar.shayakhmet@gmail.com'
        gmail_password = 'sm0k3w33d3v3ryd4y'
        sent_from = gmail_user
        to = 'onemanarmy228@gmail.com'
        subject = 'Face was detected!!!'
        body = 'The bus number #53'
        email_text = """\
        From: %s
        To: %s
        Subject: %s

        %s
        """ % (sent_from, ", ".join(to), subject, body)
        if (confidence > 0):
            name = names[id]

            if (confidence > 40):

                cv2.imwrite('detected/' + name + '.jpg', img)

                try:
                    context = ssl.create_default_context()
                    server = smtplib.SMTP_SSl('smtp.gmail.com', 465)
                    server.ehlo()
                    server.starttls(context)
                    server.login(gmail_user, gmail_password)
                    server.sendmail(sent_from, to, email_text)
                    server.close()

                    print('Email sent!')

                except:
                    print('Something went wrong...')

            confidence = "  {0}%".format(round(confidence + 20))

        else:
            name = "unknown"

        cv2.putText(img, str(name), (x+5,y-5), font, 1, (255,255,255), 2)
        cv2.putText(img, str(confidence), (x+5,y+h-5), font, 1, (255,255,0), 1)

    remotecv.imshow('Stream', img)

    exitButton = remotecv.waitKey(10) & 0xff # Press 'ESC' for exiting video
    if exitButton == 27:
        break

print("\n Stopping the programm ...")
cam.release()
cv2.destroyAllWindows()
