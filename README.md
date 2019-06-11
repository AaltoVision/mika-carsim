# Unity-based driving simulator

This repository contains a Unity-based driving simulator for training reinforcement learning agents.

## Features
 * Domain randomization (textures, cubemaps, driving dynamics, track properties, random objects around the track)
 * OpenAI Gym compatible

## Getting started

Build the project with Unity Editor. Make sure that `DrivingLearningBrain` is selected instead of `PlayerBrain` from the `Academy` component as well as the `Agent` component.
To use the compiled binary from Python, install `gym-unity` and `ml-agents` packages with Pip:


```$ pip install gym-unity ml-agents```


Check out `gym_example.py` for an example on how to run the simulator:
```
from gym_unity.envs import UnityEnv

env = UnityEnv('build', worker_id=0, use_visual=True)

for _ in range(10):
    env.reset()
    done = False
    while not done:
        action = env.action_space.sample()
        obs, reward, done, _ = env.step(action)
        print(reward)
```

## Configuration
There are various configuration options that can be altered in the Editor. The `Academy` component contains the following options:
 * `Width` : Observation width in pixels
 * `Height`: Observation height in pixels
 * `Quality Level`: Rendering quality
 * `Time Scale`: Speed at which the simulation is run. There are some problems with higher values (over 10)
 * `Target Frame Rate`
 * `Reset Parameters`: These parameters control the probabilities ([0, 1]) of randomizing specific aspects on episode reset. The default values can be set here, and they can be altered at run-time from the Python interface.

In addition, the `Agent` component contains the following options:
 * `Decision Interval`: Action repeat, i.e. how many steps should one action be performed for before requesting a new one

### Reset parameters
**Note:** this feature requires a patched version of `gym-unity`, which can be found [here.](https://github.com/kekeblom/ml-agents)


Reset parameters allow run-time modification of randomization probabilities for different aspects of the simulator. Each parameter is a value between 0 and 1, and represents the probability of randomizing that aspect when the environment is reset at the end of an episode.
The environments `reset` function takes a `dict` of Reset Parameters as a keyword argument. For example:
```
env.reset(reset_params={'random_track': 1., 'random_texture': 1., 'random_fov': 1., 'random_cubemap': 1.})
```

List of available parameters:
 * `random_track`: Randomize track shape and width
 * `random_texture`: Randomize all textures (track, ground, etc.)
 * `random_fov`: Randomize camera field of view
 * `random_cubemap`: Randomize skybox cubemap
 * `random_camera_height`: Randomize camera height from ground
 * `random_dynamics`: Randomize various car dynamics (torque, steering angle, top speed, etc.)
