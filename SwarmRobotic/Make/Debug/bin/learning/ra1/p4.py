import tensorflow as tf
import math
import numpy as np
import time
import random
from tensorflow.python.framework import ops
from tensorflow.python.framework import tensor_shape
from tensorflow.python.framework import tensor_util
from tensorflow.python.ops import math_ops
from tensorflow.python.ops import random_ops
from tensorflow.python.ops import array_ops

tf.set_random_seed(0)

### model class and key function

#activation function 
def selu(x):
    alpha = 1.6732632423543772848170429916717
    scale = 1.0507009873554804934193349852946
    return scale*tf.where(x>=0.0, x, alpha*tf.nn.elu(x))


#hidden layers or linear_output layer
class HiddenLayer(object):
    def __init__(self, input, n_in, n_out, W=None, b=None, activation=None):
        self.n_in = n_in
        self.n_out = n_out
        self.input = input
        if W is None:
            W = tf.Variable(tf.truncated_normal([n_in, n_out], stddev= tf.sqrt(1.0/n_in)))
        if b is None:
            b = tf.Variable(tf.zeros([n_out]))
        self.W = W
        self.b = b
        lin_output = tf.matmul(input, self.W) + self.b
        
        if activation is None:
            self.output = lin_output
        else:
            self.output = tf.nn.dropout(activation(lin_output), pkeep)

    def rms_loss(self, y):
        return tf.sqrt(tf.reduce_mean(tf.square(self.output-y)))

#stacked auto-encoder
class SAE(object):
    def __init__(self, n_in=30, n_out=2, hidden_layers_sizes=[128, 64, 32, 16]):
        self.x = tf.placeholder(tf.float32, [None, 30])
        self.y = tf.placeholder(tf.float32, [None, 2])
        self.lr = tf.placeholder(tf.float32)

        self.encoder_layers = []
        self.decoder_layers = []

        self.n_layers = len(hidden_layers_sizes)
        decoder_layers_sizes = [1] * self.n_layers 
        for i in range(self.n_layers):
            decoder_layers_sizes[i] = hidden_layers_sizes[self.n_layers-2-i]
        decoder_layers_sizes[-1] = n_in

        encoder_Ws = []
        encoder_bs = []
        decoder_Ws = []
        decoder_bs = []

        for i in range(self.n_layers):
            if i == 0:
                input_size = n_in
                layer_input = self.x
            else:
                input_size = hidden_layers_sizes[i-1]
                layer_input = self.encoder_layers[-1].output
            
            encoder_layer = HiddenLayer(input = layer_input, n_in = input_size, n_out = hidden_layers_sizes[i], activation = selu)
            self.encoder_layers.append(encoder_layer)
            encoder_Ws.append(self.encoder_layers[-1].W)
            encoder_bs.append(self.encoder_layers[-1].b)

        for i in range(self.n_layers):
            if i == 0:
                input_size = hidden_layers_sizes[-1]
                layer_input = self.encoder_layers[-1].output
            else:
                input_size = decoder_layers_sizes[i-1]
                layer_input = self.decoder_layers[-1].output

            decoder_layer = HiddenLayer(input = layer_input, n_in = input_size, n_out = decoder_layers_sizes[i], activation = selu)
            self.decoder_layers.append(decoder_layer)
            decoder_Ws.append(self.decoder_layers[-1].W)
            decoder_bs.append(self.decoder_layers[-1].b)

        self.pretrain_cost = tf.sqrt(tf.reduce_mean(tf.square(self.x - self.decoder_layers[-1].output)))
        p_opt = tf.train.AdamOptimizer(self.lr)
        p_grads_and_vars = p_opt.compute_gradients(self.pretrain_cost, encoder_Ws + encoder_bs + decoder_Ws + decoder_bs)
        p_grads_and_vars = [(tf.clip_by_value(gv[0], -1.0, 1.0), gv[1]) for gv in p_grads_and_vars]
        self.pretrain_step = p_opt.apply_gradients(p_grads_and_vars)

        self.linearLayer = HiddenLayer(input = self.encoder_layers[-1].output, n_in = hidden_layers_sizes[-1], n_out = n_out)
        output_Ws = [self.linearLayer.W, self.linearLayer.b]
        self.finetune_cost = self.linearLayer.rms_loss(self.y)
        f_opt = tf.train.AdamOptimizer(self.lr)
        f_grads_and_vars = f_opt.compute_gradients(self.finetune_cost, encoder_Ws + encoder_bs + output_Ws) 
        f_grads_and_vars = [(tf.clip_by_value(gv[0], -1.0, 1.0), gv[1]) for gv in f_grads_and_vars]
        self.finetune_step = f_opt.apply_gradients(f_grads_and_vars)
            

### model object
pkeep = tf.placeholder(tf.float32)
sae = SAE(n_in = 30, n_out = 2, hidden_layers_sizes = [128, 256, 256, 128])


### session configuration
init = tf.group(tf.global_variables_initializer(), tf.local_variables_initializer())
config = tf.ConfigProto()
config.gpu_options.per_process_gpu_memory_fraction = 0.105
sess = tf.Session(config = config)
sess.run(init)

### session code
mean_decode = np.zeros(32)
std_decode = np.zeros(32)
p = open('md.txt')
i = 0
for i in range(32):
    line = p.readline()
    lines = line.strip('{},\n').split(',')
    mean_decode[i] = float(lines[0])
    std_decode[i] = float(lines[1])
p.close()

print(mean_decode)
print(std_decode)

#data_3-2
print("Training:")
count = 0
pp = 0.97
average_train_loss = 0.0

best_validation_loss = float('inf')
corresponding_test_loss = 0.0
best_test_loss =  float('inf')
best_vali_num = 0
best_test_num = 0


#curr_x_validation_batch, curr_y_validation_batch = sess.run([x_validation_batch, y_validation_batch])
vt0 = time.time()
validation_data = np.loadtxt('data_validation.csv', delimiter = ',')
curr_x_validation_batch = validation_data[:, :30]
curr_y_validation_batch = validation_data[:, 30:]
curr_x_validation_batch = (curr_x_validation_batch - mean_decode[:30]) / std_decode[:30]
vt1 = time.time()
print("load validation " + str(vt1-vt0) + " seconds")
#curr_y_validation_batch = curr_y_validation_batch - mean_decode[30:]


#curr_x_test_batch, curr_y_test_batch = sess.run([x_test_batch, y_test_batch])
tt0 = time.time()
test_data = np.loadtxt('data_test.csv', delimiter = ',')
curr_x_test_batch = test_data[:, :30]
curr_y_test_batch = test_data[:, 30:]
curr_x_test_batch = (curr_x_test_batch - mean_decode[:30]) / std_decode[:30]
tt1 = time.time()
print("load test " + str(tt1-tt0) + " seconds")
#curr_y_test_batch = curr_y_test_batch - mean_decode[30:]

nt0 = time.time()
train_data = np.loadtxt('data_train.csv', delimiter = ',')
batch_size = 500
max_line = train_data.shape[0]
#batch_num = max_line / batch_size
max_index = max_line - batch_size
print("max_index "+str(max_index))
nt1 = time.time()
print("load train " + str(nt1-nt0) + " seconds")

'''
# prepare
learning_rate = 0.001
#dropout_4-4
max_learning_rate = 0.003
min_learning_rate = 0.000001
decay_speed = 100000.0 
average_cost = 0.0

for i in range(500000):
    #pt0 = time.time()
    index = random.randint(0, max_index)
    #if i < 2000:
    #    print("start " + str(index) + " --------- end" + str(index+batch_size))
    #index = int((i % batch_num) * batch_size)
    pre_x_train_batch = train_data[index:index+batch_size, :30]
    pre_x_train_batch = (pre_x_train_batch - mean_decode[:30]) / std_decode[:30]
    learning_rate = min_learning_rate + (max_learning_rate - min_learning_rate) * math.exp(-i/decay_speed)
    #pt1 = time.time()
    #print("prepare " + str(pt1-pt0) + " seconds")
    
    #pt4 = time.time()
    _, pre_loss = sess.run([sae.pretrain_step, sae.pretrain_cost], {sae.x: pre_x_train_batch, sae.lr: learning_rate, pkeep: 1.0})
    average_cost += pre_loss
    #pt5 = time.time()
    #print("train " + str(pt5-pt4) + " seconds")

    if i % 200 == 0:
        print("Pretraining batch %d, cost %f, lr %f" % (i, average_cost/200, learning_rate))
        average_cost = 0
'''


count = 0
max_learning_rate = 0.003
min_learning_rate = 0.000001
decay_speed = 100000.0 #original 1000.0

while count < 1000000:

    #index = int((count % batch_num) * batch_size)
    index = random.randint(0, max_index)
    curr_x_train_batch = train_data[index:index+batch_size, :30]
    curr_y_train_batch = train_data[index:index+batch_size, 30:]
    #curr_x_train_batch, curr_y_train_batch = sess.run([x_train_batch, y_train_batch])
    curr_x_train_batch = (curr_x_train_batch - mean_decode[:30]) / std_decode[:30]
    #curr_y_train_batch = curr_y_train_batch - mean_decode[30:]

    learning_rate = min_learning_rate + (max_learning_rate - min_learning_rate) * math.exp(-count/decay_speed)

    train_loss = sess.run(sae.finetune_cost, {sae.x: curr_x_train_batch, sae.y: curr_y_train_batch, pkeep: 1.0})
    average_train_loss += train_loss

    #data_3-3
    if count % 200 == 0:
        print(str(count)+" loss/average_loss: "+str(train_loss)+"/"+str(average_train_loss/200)+"(lr:" + str(learning_rate)+")" )
        average_train_loss = 0
        
        validation_loss = sess.run(sae.finetune_cost, {sae.x: curr_x_validation_batch, sae.y: curr_y_validation_batch, pkeep: 1.0})
        if validation_loss < best_validation_loss:
            best_vali_num = count
            best_validation_loss = validation_loss
            with open("parameters-snn-temp4.txt", "w") as outfile:
                np.set_printoptions(threshold=np.NaN)
                for i in range(sae.n_layers):
                    outfile.write(str(sae.encoder_layers[i].W.eval(session=sess))+'\n')
                    outfile.write(str(sae.encoder_layers[i].b.eval(session=sess))+'\n')
                outfile.write(str(sae.linearLayer.W.eval(session=sess))+'\n')
                outfile.write(str(sae.linearLayer.b.eval(session=sess)))

            corresponding_test_loss = sess.run(sae.finetune_cost, {sae.x: curr_x_test_batch, sae.y: curr_y_test_batch, pkeep: 1.0})

            if corresponding_test_loss < best_test_loss:
                best_test_num = count
                best_test_loss = corresponding_test_loss

            print("flag "+str(count)+" bv_loss: "+str(best_validation_loss) +"###t_loss/bt_loss: "+str(corresponding_test_loss)+"/"+str(best_test_loss))
        #else:
        #    learning_rate *= 0.993
            

    if count % 2000 == 0:
        print("h "+str(count)+" bv_loss/ct_loss/best_vali-----bt_loss/best_test: "+str(best_validation_loss) +"/"+str(corresponding_test_loss)+"/"+str(best_vali_num)+"------"+str(best_test_loss) +"/"+str(best_test_num))

    count += 1
    sess.run(sae.finetune_step, {sae.x: curr_x_train_batch, sae.y: curr_y_train_batch, sae.lr: learning_rate, pkeep: pp})
            

print("Last "+str(count)+" bv_loss/ct_loss/best_vali-----bt_loss/best_test: "+str(best_validation_loss) +"/"+str(corresponding_test_loss)+"/"+str(best_vali_num)+"------"+str(best_test_loss) +"/"+str(best_test_num))
sess.close()

