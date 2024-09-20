import numpy as np

data = np.loadtxt('data_train.csv', delimiter = ',')
mean = np.mean(data, 0)
std = np.sqrt(np.var(data, 0))
f = open('md.txt', 'w')
for i in range(mean.shape[0]):
    f.write('{'+str(mean[i])+','+str(std[i])+'},\n')
#    print("{%f,%f},"%(mean[i], std[i]))
f.close()

print(mean)
print(std)

#mean.shape[0] default 32
size = mean.shape[0]
mean_decode = np.zeros(size)
std_decode = np.zeros(size)
p = open('md.txt')
i = 0
for i in range(size):
    line = p.readline()
    lines = line.strip('{},\n').split(',')
    mean_decode[i] = float(lines[0]) 
    std_decode[i] = float(lines[1])
p.close()

print(mean_decode)
print(std_decode)
