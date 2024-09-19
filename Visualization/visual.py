import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.animation import FuncAnimation
import numpy as np

class Object:
    def __init__(self, name, position=None, rotation=None, scale=None):
        self.name = name
        self.position = position if position is not None else np.zeros(3)
        self.rotation = rotation
        self.scale = scale
    
    def update(self, position, rotation, scale):
        self.position = position
        self.rotation = rotation
        self.scale = scale

class MultiViewer:
    def __init__(self, parent, children, scenario, objectNames):
        self.parent = parent
        self.children = children
        self.scenario = scenario
        
        self.objects = {name: Object(name) for name in objectNames}
        self.selectedObject = None
    
    def updateObject(self, name, position, rotation, scale):
        self.objects[name].update(position, rotation, scale)
    
    def changeParent(self, parent):
        oldParent = self.parent
        self.children.remove(parent)
        self.children.append(oldParent)
        self.parent = parent
    
    def selectObject(self, object):
        self.selectedObject = object

# Initialize 3D plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

scat = ax.scatter([], [], [], c='b', marker='o')
text_objs = []

ax.set_xlabel('X Coordinate')
ax.set_ylabel('Y Coordinate')
ax.set_zlabel('Z Coordinate')

def init():
    scat.set_offsets(np.empty((0, 3)))  # Empty scatter plot initially
    return scat,

# Read file utility
def read_file(file_path):
    with open(file_path, 'r') as f:
        return f.readlines()

# Parse MultiViewer object from file
def parse_multiviewer(file_path):
    mvFile = read_file(file_path)

    parent, children, scenario, objectNames = None, [], [], []

    for line in mvFile:
        if 'SETTING PARENT' in line:
            parent = line.split(" - ")[2].strip()
        elif 'SETTING CHILD' in line:
            children.append(line.split(" - ")[2].strip())
        elif 'SETTING SCENARIO' in line:
            scenario.append(line.split(" - ")[2].strip())
        elif 'SETTING OBJECT' in line:
            objectNames.append(line.split(" - ")[2].strip())

    return MultiViewer(parent, children, scenario, objectNames)

# Parse and update objects based on file data
def parse_objects(file_path, mv: MultiViewer):
    objectFile = read_file(file_path)

    for line in objectFile:
        parts = line.split(" - ")
        timeStamp = parts[0].strip()

        # Plot every 5 seconds
        if int(timeStamp.split(":")[2]) % 5 == 0:
            position = np.array([float(x) for x in parts[2].strip().split(',')])
            rotation = np.array([float(x) for x in parts[3].strip().split(',')])
            scale = np.array([float(x) for x in parts[4].strip().split(',')])
            mv.updateObject(parts[1].strip(), position, rotation, scale)

# Function to plot objects
def plot_objects(mv):
    global text_objs

    for text in text_objs:
        text.remove()
    text_objs.clear()

    # Plot new positions
    positions = np.array([obj.position for obj in mv.objects.values()])
    scat._offsets3d = (positions[:, 0], positions[:, 1], positions[:, 2])

    for i, obj in enumerate(mv.objects.values()):
        text = ax.text(obj.position[0], obj.position[1], obj.position[2], obj.name, fontsize=12)
        text_objs.append(text)
    
    return scat,

# Main function to run the animation
def animate_multiviewer(file_path_mv, file_path_obj):
    mv = parse_multiviewer(file_path_mv)
    parse_objects(file_path_obj, mv)
    
    ani = FuncAnimation(fig, plot_objects, fargs=(mv,), init_func=init, interval=5000, repeat=True)
    plt.show()
