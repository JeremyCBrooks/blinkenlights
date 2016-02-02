#!/usr/bin/python
import os

fifo = "/tmp/blink.fifo"
try:
   os.mkfifo(fifo)
except:
   pass

x = ""
while x!="x":
   f = open(fifo, "r")
   x = f.read(1)
   f.close()
   if x == "1":
      print "RED"
   elif x == "2":
      print "GREEN"
   elif x == "3":
      print "BLUE"
   elif x == "0":
      print "All off"
   elif x == "x":
      print "Shutdown"
   elif x != "":
      print "unrecognized command"
