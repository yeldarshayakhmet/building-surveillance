This project was developed for World Robot Olympiad 2019 Kazakhstan. The recognition and people-counting folders contain algorithms that are run on Raspberry Pi hardware equipped with a camera (Raspbian OS). A Windows PC, where the C# application is run, connects to the Raspberry device through LAN. It runs the Python algorithms through SSH, reads file output through SFTP, and receives video streaming from the Raspberry camera through Gstreamer third party software.
The aim of the face-recognition algo is to detect wanted criminals in buildings, while the people-counting algorithm keeps track of the number of people in the building for emergency situations. Their code is adapted from open GitHub repositories.
