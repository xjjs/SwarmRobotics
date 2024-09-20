import numpy as np
import re

for i in range(9):
    inname = 'parameters-snn-temp'+str(i+1)+'.txt'
    outname = 'parameters-snn-temp'+str(i+1)+'m.txt'
    inFile = open(inname, 'r')
    outFile = open(outname, 'w')
    line = inFile.readline()
    while line:
        flag = False
        if ']' in line:
            flag = True
        items = re.split(r'[\s]\s*', line.strip('][\n '))
        for item in items:
            outFile.write(str(item)+' ')
        if flag:
            outFile.write('\n')
        line = inFile.readline()


    #for i in range(mean.shape[0]):
    #    f.write('{'+str(mean[i])+','+str(std[i])+'},\n')
    #    print("{%f,%f},"%(mean[i], std[i]))
    inFile.close()
    outFile.close()

