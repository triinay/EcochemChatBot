#!/usr/bin/env python
# coding: utf-8

# In[1]:


import imageio
import matplotlib.pyplot as plt
import numpy as np
from sklearn.cluster import KMeans
import cv2
from skimage.color import rgb2lab, deltaE_cie76
from collections import Counter
import os 
from scipy.spatial import KDTree
from webcolors import (
    CSS3_HEX_TO_NAMES as css3_hex_to_names,
    hex_to_rgb,


# In[4]:


class Color_Image:
    def __init__(self, filepath: str):
        self.path = filepath
        self.image = cv2.cvtColor(cv2.imread(filepath), cv2.COLOR_BGR2RGB)
        
    @staticmethod
    def RGB_HEX(color):
         return "#{:02x}{:02x}{:02x}".format(int(color[0]), int(color[1]), int(color[2]))
    
    def get_colors(self, number_of_colors):
        reshaped_image = cv2.resize(self.image, (600, 400))
        reshaped_image = reshaped_image.reshape(reshaped_image.shape[0]*reshaped_image.shape[1], 3)
        
        clf = KMeans(n_clusters = number_of_colors)
        labels = clf.fit_predict(reshaped_image)
        
        counts = Counter(labels)
        counts = dict(sorted(counts.items()))
        
        center_colors = clf.cluster_centers_
        
        ordered_colors = [center_colors[i] for i in counts.keys()]
        hex_colors = [self.RGB_HEX(ordered_colors[i]) for i in counts.keys()]
        rgb_colors = [ordered_colors[i] for i in counts.keys()]
        
        #if (show_chart):
            #plt.figure(figsize = (8, 6))
            #plt.pie(counts.values(), labels = hex_colors, colors = hex_colors)
        return rgb_colors 


# In[10]:


image = Color_Image('grey.JPG')
colors = image.get_colors(8)


# In[8]:


def convert_rgb_to_names(rgb_tuple):
    
    css3_db = css3_hex_to_names
    names = []
    rgb_values = []
    
    for color_hex, color_name in css3_db.items():
        names.append(color_name)
        rgb_values.append(hex_to_rgb(color_hex))
    
    kdt_db = KDTree(rgb_values)
    distance, index = kdt_db.query(rgb_tuple)
    return names[index]


# In[11]:


names = [convert_rgb_to_names(tuple(color)) for color in colors]


# In[16]:


import pandas as pd

ral_colors = pd.read_csv('ral_colors.csv')


# In[13]:


hex_ral = ral_colors[['ral','english']]


# In[14]:


import re


# In[15]:


fitting_ral = []
for name in names:
    for color_ral in hex_ral.english:
        if re.match(name,color_ral):
            color_list = hex_ral[hex_ral['english'] == color_ral].values
            fitting_ral.append(color_list[0][0])
', '.join(fitting_ral)

