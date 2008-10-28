
NModel for Model-Based Testing of Web Applications

This is a simple example of how to use NModel toolkit
to test the functionality of a web site. The model 
is composed of three different functionalities ---
features which can be composed into models.

The sample consists of an implementation under test,
which is contained in the directory called php.
The model of the behaviour is contained in Model.
The test harness sometimes also called adapter is contained
in Adapter.

Setup

The web application is located in the folder php. Please copy the files
to a web server supporting PHP. Then modify the corresponding URLs in
the site specific configuration file Adapter/Site1.cs or Adapter/Site2.cs.

To install NModel, please download the NModel.msi from 
http://www.codeplex.com/NModel

You will currently also need a GLEE library for MPV that can be down-
loaded from a location referenced there.

After installation copy all files from

c:\Program Files\NModel\bin
to 
WebApplication\build

and 
c\Program Files\Microsoft Research\GLEE\bin
to
WebApplication\build

and build the model and the adapter either by using Visual Studio 2005
or the provided build*.bat or build*.sh scripts. To build you will need
to have either .NET SDK 2.0 or Mono 1.2.3 or later installed.


Login - logout

This feature models logging in and out of a web site.


Update integer values on the web page

The simple web based application has two forms:
on one of them it is possible to insert integers and on the other strings.
These have been modelled in different features and can be tested either
in one test run or separately. The strings model is only a stub, the
functionality needs to be added.


Exploring the models

The models can be explored using Model Program Viewer, mpv.exe. Please
use the provided response files for viewing different combinations
of features.

mpv @mpv_argsN.txt


Online conformance testing

Online testing can be started with the following set of options:

ct.exe @ct_args_login.txt
ct.exe @ct_args_int.txt