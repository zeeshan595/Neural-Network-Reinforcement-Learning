# Neural-Network-Reinforcement-Learning
This project is created in c# using Unity3D Game Engine. In the project, a neural network is developed, and a car uses that neural network to train itself to drive. Several techniques are used in the project such as RL, PSO and GA. Currently, the project uses Multi layered feed forward network, but I am planning to switch to the recurrent neural network.

Please start the project from the `menu` scene.

# RL & RL_PSO
RL and RL_PSO both techniques do not have sessions or terminate, They will both go on forever unless the car is stuck and needs to be reset. In that case, the car is sent back to the spawn position. To detect if the vehicle is stuck the velocity magnitude of the vehicle must be lower than 0.5f and at least 100 steps must be taken since the last reset.

# PSO & GA
These both meta-heuristics use multiple instances of the same network to figure out the optimal weights. Although these methods can be very efficient, they are not very good for some applications where you can only have one instance.

# Build using Unity 3D
To open or build the project please download a copy of unity and open the folder using unity3d.
Unity3D: https://unity3d.com/