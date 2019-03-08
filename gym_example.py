from gym_unity.envs import UnityEnv

env = UnityEnv('build', worker_id=0, use_visual=True)

for _ in range(10):
    env.reset()
    done = False
    while not done:
        action = env.action_space.sample()
        obs, reward, done, _ = env.step(action)
        print(reward)

