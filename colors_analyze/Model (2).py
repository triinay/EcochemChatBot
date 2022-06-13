#!/usr/bin/env python
# coding: utf-8

# In[21]:


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
    hex_to_rgb)
import requests
import re
import vk_api
from vk_api.longpoll import VkLongPoll, VkEventType
import pandas as pd


# In[45]:


def get_image(url: str) -> str:
    r = requests.get(url)
    with open('color_to_analyze.jpg', 'wb') as f:
        f.write(r.content)
    return 'color_to_analyze.jpg'

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

        return rgb_colors

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

ral_colors = pd.read_csv('ral_colors.csv')
hex_ral = ral_colors[['ral','english']]

def fitting_rals():
    fitting_ral = []
    for name in names:
        for color_ral in hex_ral.english:
            if re.match(name,color_ral):
                color_list = hex_ral[hex_ral['english'] == color_ral].values
                fitting_ral.append(color_list[0][0])
    return ', '.join(list(set(fitting_ral)))


# In[47]:


token = '0e8fe479d08ebd0e74d84446c5b8459d2ef17c3cdea489b593d91236ccf818100c11a5b963e04b10a680a'
vk_session = vk_api.VkApi(token = token)
vk = vk_session.get_api()
longpoll = VkLongPoll(vk_session)

def sender(id, text):
    vk.messages.send(user_id = id, message = text, random_id = 0)
    
for event in longpoll.listen():

    if event.type == VkEventType.MESSAGE_NEW and event.to_me:
            if event.attachments:

                msg = event.attachments['attach1_type']

                id = event.user_id

                if msg == 'photo':
                    items = vk.messages.getById(message_ids=event.message_id)
                    item_url = items["items"][0]["attachments"][0]["photo"]["sizes"][4]["url"]
                    filename = get_image(item_url)
                    image = Color_Image(filename)
                    colors = image.get_colors(8)
                    names = [convert_rgb_to_names(tuple(color)) for color in colors]
                    ral = fitting_rals()
                    sender(id, f'Возможно, Вам подойдут следующие цвета RAL: {ral}')
                else:
                    sender(id, "Некорректный ввод. Пожалуйста, отправьте фото в формате jpg")
            elif event.text == "Начать" or event.text == "Start":
                sender(id, "Для получения автоматического определения подходящего цвета RAL, пожалуйста, отправьте фото в формате jpg")
            else:    
                sender(id, "Некорректный ввод. Пожалуйста, отправьте фото в формате jpg")


# In[ ]:




