<?php

//start the session
session_start();

//check to make sure username has been set in session
if(isset($_SESSION['username'])){

//We are logged in.

//the session variable is registered, the user is allowed to see anything that follows

$num=0;

if (isset($_SESSION['num']))
	$num=$_SESSION['num'];
if (isset($_GET['num'])){
	$num=intval($_GET['num']);
        $_SESSION['num']=$num;
}

$str="";

if (isset($_SESSION['str']))
	$str=$_SESSION['str'];
if (isset($_GET['str'])){
	$str=$_GET['str'];
        $_SESSION['str']=$str;
}


///////////////////////////// 
// HTML that displays two forms:
// * A form to enter a number, and
// * A form to enter a string.
// The entered value is stored in
// the session and is displayed 
// on the web page.
/////////////////////////////
?>
<html>
<head>
<title>DoStuff</title>
</head>
<body>
<?
echo "Number: ".$num;
echo "<br/>";
echo "String: ".$str;
?>
<form name="number" method="GET" action="doStuff.php">
Number: <input type="text" name="num" size="2">
<input type="submit" value="Submit" name="inputNumber">
</form>
<form name="string" method="GET" action="doStuff.php">
String: <input type="text" name="str" size="20">
<input type="submit" value="Submit" name="inputString">
</form>

<a href="logout.php">Log out</a>
</body>
</html>
<?
////////////////////////// End of DoStuff//////////////////
}
else{

///////////////////////// Display login form //////////////
if (!isset($_POST["username"]) || !isset($_POST["password"])) {

?>
<html>
<head>
<title>LoginPage</title>
</head>
<body>
<form method="POST" action="doStuff.php">
Username: <input type="text" name="username" size="20">
Password: <input type="password" name="password" size="20">
<input type="submit" value="Submit" name="login>
</form>
</body>
</html>


<?
//////////////////////// End login form ////////////////////
 
} else{

$user = addslashes($_POST['username']);
$pass = addslashes($_POST['password']);

$userDB = array("user1"=>"123", "user2"=>"234", "user3"=>"345");


if (isset($userDB[$user]) && $userDB[$user]==$pass) {

  //start the session and register a variable

  session_start();
  $_SESSION['username']=$user;
  $_SESSION['password']=md5($password);

  //we will redirect the user to another page where we will make sure they're logged in
  header( "Location: doStuff.php" );

  }
  else {

  //if nothing is returned by the query, unsuccessful login code goes here...

  echo 'Incorrect login name or password. Please try again.';
  }
  }



}
?>
