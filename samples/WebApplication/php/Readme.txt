
These PHP files constitute the implementation under test.
They feature a web page where it is possible to post strings
and numbers. The page should implement persistence between logins
but it does not. NModel catches this imperfection.

For testing copy these to a folder where a php enabled web server
can access them. Change the URLs in Site1.cs in the Adapter directory.
Or, you can also use Site2 to test an instance of
the implemenation available at http://www.cc.ioc.ee/~juhan/nmodel/webapplication/php


