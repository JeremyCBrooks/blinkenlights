#!/usr/bin/python

def getKey():
   import sys, select

   if sys.stdin in select.select([sys.stdin], [], [], 0)[0]:
      return sys.stdin.read(1)
   return None 

import time
import os
import errno
import RPi.GPIO as GPIO

redPin = 11
greenPin = 13
bluePin = 15
pwmPin = 12
pwm = None  
dc = 0
dcDelta = 0
activePin = 0
fifo = None

def init():
   print("init start")
   GPIO.setmode(GPIO.BOARD)
   GPIO.setup(redPin, GPIO.OUT)
   GPIO.setup(greenPin, GPIO.OUT)
   GPIO.setup(bluePin, GPIO.OUT)
   GPIO.setup(pwmPin, GPIO.OUT)

   allOff()

   global pwm
   pwm = GPIO.PWM(pwmPin, 50)
   pwm.start(0)
   print("init end")

   fifoPath = "/tmp/blink.fifo"
   if not os.path.exists(fifoPath):
      os.mkfifo(fifoPath)
   global fifo
   fifo = os.open(fifoPath, os.O_RDONLY|os.O_NONBLOCK)

def readFIFO():
   try:
      buffer = os.read(fifo, 1)
   except OSError as err:
      if err.errno == errno.EAGAIN or err.errno == errno.EWOULDBLOCK:
         buffer = None
      else:
         raise  # something else has happened
   if len(buffer)==0:
      return None
   else:
      print "stuff: ", buffer
      return buffer

def allOff():
   GPIO.output(redPin, False)
   GPIO.output(greenPin, False)
   GPIO.output(bluePin, False)
   print("All pins off") 

def enablePin(pin):
   global dc
   global dcDelta
   global activePin

   allOff()

   if pin == redPin:
      dcDelta = 5
      dc = 0
   elif pin == greenPin:
      dcDelta = 0
      dc = 100
   elif pin == bluePin:
      dcDelta = 1
      dc = 0

   activePin = pin
   pwm.ChangeDutyCycle(dc)
   GPIO.output(activePin, True)

   print("Pin %d on" % pin)

try:
   init()

   while True:
      pwm.ChangeDutyCycle(dc)
      dc += dcDelta 
      if dc > 100:
         if activePin == bluePin:
            dc = 100
            dcDelta = -dcDelta
         else:
            dc = 0

      if dc < 0:
         dc = 0
         dcDelta = -dcDelta

      x = readFIFO() #getKey()
      if(x is not None):
         print(x)

         if x=='x':
            break
         elif x=='1':
            enablePin(redPin)
         elif x=='2':
            enablePin(greenPin)
         elif x=='3':
            enablePin(bluePin)
         elif x=='0':
            allOff()
         else:
            print("Unknown command")

      time.sleep(0.05)
except Exception, err:
   print Exception, err
finally:
   #cleanup
   if fifo is not None:
      print "close fifo"
      os.close(fifo)

   allOff()

   if pwm is not None:
      print "pwm stop"
      pwm.ChangeDutyCycle(0)
      pwm.stop()

   GPIO.cleanup()
   print "GPIO cleanup"

   print "exit"
