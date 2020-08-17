# Swarms

## Getting Started

Download the zip file from the repository

Extract it to a convenient location

Download Version “2019.1.8f1” from the Hub

“Add” project folder

Open project.

In the “Scenes” Folder, Open the X Scene

Try running the simulation with the play button

Check for compiler errors

## Changing the Simulation

To change the amount of boids, click on the spawner object in the hierarchy and change "Number of boids" in the inspector.
You can change the size of the box by increasing the scale components in the transform of the “Level” object.

If you want to toggle the third person camera in the simulation, press the “Space” Key
(You can change this Key in CameraSwitch.cs)

While the Sim is running, go into the Settings folder and click on settings

In the top right you should see all the settings from the Scripts/Settings.cs file.

Here you can change the parameters to see how the FishBoids behaviour changes

What happens when you change the perception radius from 0.1 to 10?

Note: If you set the Steer force to less than about 2 then they have trouble steering in time to avoid the walls and they’ll glitch through it. Anywhere from 4 - 6 is a good value.

Interestingly, if you set the Steer force to a really high value then they start to slow down, what could be the reason for this?

Try changing the weights from 0.1 to 1 - Values above 1.5 tend to get a bit messy

If you would like to change things at the code level I've documented the code in FishBoids.cs and Settings.cs to help you better understand whats going on. I think these are the files that would be most useful to change.


### I have used and/or modified code in parts of this project under the following license
  
>MIT License
>
>Copyright (c) 2019 Sebastian Lague
>
>Permission is hereby granted, free of charge, to any person obtaining a copy
>of this software and associated documentation files (the "Software"), to deal
>in the Software without restriction, including without limitation the rights
>to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
>copies of the Software, and to permit persons to whom the Software is
>furnished to do so, subject to the following conditions:
>
>The above copyright notice and this permission notice shall be included in all
>copies or substantial portions of the Software.
