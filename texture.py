import numpy as np
from PIL import Image

width = 500
lineWidth = 25

line_color = np.ones((width, lineWidth, 3))
track_color = np.array([0.0, 1.0, 0.0])
ground_color = np.array([1.0, 0.0, 0.0])
line_color = np.array([0.0, 0.0, 1.0])

tex = np.ones((width, width, 3)) * track_color
tex[:, :lineWidth, :] = line_color
tex[:, -lineWidth:, :] = line_color
tex += np.random.uniform(-0.3, 0.3, tex.shape)
tex = tex.clip(0, 1.0)

Image.fromarray((tex * 255).astype(np.uint8)).save('./texture.png')
