---
layout: post
title:  "Fagor Ederlan IoT Hackfest"
author: "Juan Manuel Servera"
author-link: "#"
#author-image: "{{ site.baseurl }}/images/authors/jmservera.jpg"
date:   2016-11-08
categories: IoT
color: "blue"
#image: "{{ site.baseurl }}/images/imagename.png" #should be ~350px tall
excerpt: An Industry 4.0 project for gathering data secureyly from an aluminum molding machine to create a ML model to discard faster the defective pieces and then improve the process. The data is sent compressed to Azure through an IoT connection and prepared with Azure Functions, then sent to a ML model from Stream Analytics and presented in a PowerBI.
language: English
verticals: Manufacturing
---

## Enhancing a molding process with Industry 4.0 solutions ##

In the third wave of industrial evolution we had automation that produced big amounts of data. This data has high potential of analytic uses, but it was not easy to analyze,
because it was siloed in the machines where it was generated.
With this project we will demonstrate that it's not complex to send the data to the cloud, using secure and reliable services, that allow us to analyze the data in near-realtime and build ML models to extract knowledge from it.

In this project will capture the data produced during the molding process to build a ML model that will warn us when the quality of the product may be lower than the defined standard.
While in a canonical scenario, the data is gathered directly from the sensors (or via PLC) and send to the cloud, directly or via a gateway, in many manufacturing projects we have to deal with old and/or proprietary systems that cannot be directly connected. Sometimes you don't have a system with connectivity, in other cases you don't have the protocols to communicate with the sensors.

The machine where we will deploy this prototype didn't have a network connection, and the sensors are connected directly to the machine. The control software already gathers all the sensors information as *CSV* files and for each piece it calculates averages, means and some other statistical values from the data. As we wanted to minimize the impact we have in the machine, we developed a small windows service that detected new *CSV* files in a folder and sent them zipped to IoT Hub, all the rest of the process is executed in the cloud side.

![Robot and Mold]({{ site.baseurl }}/images/fagorederlan/robotAndMold.jpg)

Two ML models will be built with the gathered data:
- The first one will be built on the data from in-mold sensors, and will be correlated with a dataset of defective parts. This will greatly enhance the project, because it will allow to do early detection of defective pieces.  
- In the future, the plan is to build a model to enhance the molding process directly, but for this we will need to retrieve the molding parameters as well.

The main Technologies used are:
* IoT Hub, to receive sensor data as events and files
* Intel Curie, to gather environmental data
* Azure Functions, to uncompress data and do some maths on the data
* Azure Stream Analytics, to join the datasets and ask the ML model
* Azure ML, to build the ML models and create the API endpoints

As Jesús Para, main Data Scientist in Fagor Ederlan, indicated:

> "Being able to early detect one defective piece before it arrives to our customers will pay the Azure consumption for one month".


## Customer ##
**[Mondragón](http://www.mondragon-corporation.com/eng)** is one of the leading Spanish business groups, integrated by autonomous and independent cooperatives with production subsidiaries and
corporate offices in 41 countries and sales in more than 150.

**[Fagor Ederlan](http://www.fagorederlan.es)** is a company under this business group – a leading supplier of complete solutions for the automotive industry, specializing in Chassis and
Powertrain applications.
Their main installation is in the [Mondragón](https://es.wikipedia.org/wiki/Mondrag%C3%B3n) town, in the Basque Country, Spain. This is where we have developed and deployed the prototype,
but one big concern is how to scale this to the whole company, with a focus in security and in working scenarios with very low bandwidth.

The participants were from the three companies that are working on the project: **Fagor Ederlan**, **LKS** and **ETIC**.

**[LKS](http://www.lks.es)** is the SI that is helping Fagor Ederlan building the ML models to improve their manufacturing process.

**[ETIC](http://www.embedded-technologies.org/en-us)** (Smart Cities Innovation Center) is a non-profit business service co-operative specialised in the development of products,
services and applications within the context of Smart Cities. By virtue of a framework agreement, it operates as a Microsoft Innovation Center.
Located in the Mondragón town as well, they are working along with LKS in this project, providing their knowledge in building IoT solutions in Azure with PowerBI and Machine Learning.

The main hack team has been formed by:

- [Fagor Ederlan](http://www.fagorederlan.es)
    - Eber Arregui (Project Manager)
    - Yolanda Mendi (IT)
    - Ibai Peña (IT)
    - Jesús Para (Data Analytics)
    - Imanol Santos (Installation technician)
- [LKS](http://www.lks.es)
    - Vicente Briz
    - Miguel Baroja
- [ETIC](http://www.embedded-technologies.org)
    - Natividad Herrasti (Managing Director)
    - Development Team
        - Aitor Gomez
        - Aitor Akizu 
        - Jon Alza
        - Josu Lopez 
        - Maite Beamurgia (Data Scientist)
- Microsoft
    - Juan Manuel Servera. Senior Technical Evangelist. ([@jmservera](http://twitter.com/jmservera))

The Hackfest was focused on three different matters:
- Learn how to use IoT Hub, how it enhances the real-time capture of the data and solves the security concerns of the company.
- Create a windows service to gather actual data using the technology that they are using nowadays, but sending it to the cloud to help analyzing it.
- Create a simulator that uses samples of the real for a prototype of the full solution with all the data management, real-time analytics, ML and visualization.
This simulator allows us to test everything without 
 
## Pain point ##

The solution they are building has the objective of early identification of defective pieces in an aluminum molding machine, by finding pores, fill levels (or lack of fill),
and act in base of the gathered knowledge.
During the injection process, the machine takes many parameters per millisecond, like speed, pressure & injector run. This creates an 800KB *CSV* file with all the measures
and another one of averages.
The process of filling the die and cooling the piece takes about 90 seconds, that can be enhanced by improving the process manually.
They have found that this data is not enough to identify the defective pieces, so they are also installing thermographic cameras that will take one picture for each stage of the process.
The thermographic images are processed locally and filed in an Access database.

When the piece is finished, a worker does a visual examination of the piece to detect defective pieces, but this does not guarantee that a piece is not defective.
The main identification of defective pieces is done at assembly time, that is usually done one-month after the piece is built.

The main issues they were facing were:
- The molding and data gathering is controlled by a proprietary system that cannot be modified
- We cannot read directly from the sensors, we can just take the *CSV* files regularly
- The PC was using Windows XP and it had no network connection
- The data gathering is not complete until the piece is assembled one-month after

Some of the issues were alleviated by the company by upgrading their systems, but some of them will be done in the future:
- The molding controller was upgraded with a more powerful machine with Windows 7
- In the future, they will evaluate new IO-Link compatible sensors so data could be retrieved in real-time using an OPC-UA gateway

Other concerns of the company were:
- Security: in this project the IT department of the company is also involved, and they are concerned about the security of the whole system
- Connectivity and bandwidth limits: their current broadband network is very limited, so they want to limit bandwidth usage of the system and control when the data is sent to the cloud
to avoid having bandwidth problems with their other systems.
- Generalization: we are going to create a pilot over one molding process, but they have a very broad range of pieces they build,
so we should think on what parts of the solution are reusable and what should be built or modified in every case.
- Costs of the whole system and scalability of the system to the full manufacturing plant
 
## Solution ##

The solution we built was focused on the data gathering, we agreed that the ML project will be a future step, even though they already have a small ML that we could fill with the data.

We built the prototype in incremental steps, and opened to build a real-time solution in the future, the one that will be used when the IO-Link sensors are installed.

### Hackfest agenda ###

During the first meetings we agreed on an agenda, that should be flexible because the access to the molding machine depended on the production needs.
We did a 5 day hackfest, split in two blocks. The first 3 days we developed all the basic parts to have a reliable solution that sends the data to the cloud.
The second block was a week after and we focused on enhancing security and reliability of the system.    

* Day 1
    * We dedicated the morning to do a [IoT Hub lab](http://thinglabs.io) to get familiar with the technology, so anyone could discuss about all the parts we were going to use.
    In the lab we used Intel Edison with Grove kits, Raspberry Pi 2 with Fez Hat and connected to the cloud using node.js and also the node-red platform.
![IotLab]({{ site.baseurl }}/images/fagorederlan/iotLab.jpg)
<img src="{{ site.baseurl }}/images/fagorederlan/iotLab.jpg" width="50px"/>
    * During the afternoon, we discussed about the technology we were going to use for each part and created an initial Kanban Board with the work for the next days.
![Kanban1]({{ site.baseurl }}/images/fagorederlan/kanban1.jpg)
* For the day 2 we divided the teams to build the different parts we needed:
    * A Windows service with a file watcher that zipped the files and connected to Azure Storage to send them 
    * 
        * Create a windows service to retrieve the current data from the molding controller (it’s a PC with windows)
        * Create a cloud service for the data management: they currently process all the data with one monolithic application, we will split the data gathering and data management in two apps. The data gathering runs in the edge and the data management and preparation is done in the cloud. We will evaluate and/or use:
            * Blob Storage
            * Message queue
            * Maybe Data Factory
            * A storage solution
            * DocumentDB
            * Azure Database
            * DataLake or Blobs?
* Day 3
    * Use Stream Analytics and PowerBI to represent the data
    * Send the data to a Database in order to fill the ML model
    * Develop a simulator that sends the raw data to IoT Hub, for preparing Day 4

* One week of working remotely on the project between day 3 and 4 

* Day 4
    * Field Gateway: as the hardware is not still prepared, we will create a small simulator that sends all the data to IoT Hub, using
        * A field simulator (from day 3)
        * IoT Hub
        * Stream Analytics
        * ML model in Real Time
        * PowerBI Embedded
        * Security (should be present in the prototype)
* Day 5
    * Conclusions
    * Prototype deployment and evaluation
    * Costs calculation


IoT proposed solution will do X and help with the pain points above by Y.

Future potential to their business and operations.

##Architecture##

In the technology diagram below you will find the different technologies we are using to get all the information from the machine.
[![Overview]({{ site.baseurl }}/images/fagorederlan/schema.svg)]({{ site.baseurl }}/images/fagorederlan/schema.svg)

##Device used & Code artifacts

##Opportunities going forward

This section is optional, but if you have details on how the customer plans to proceed or what more they hope to accomplish, please include.

##Conclusion##

Time or cost savings metrics resulting from the implementation of the solution

Changes in the company culture or their organization