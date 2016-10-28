<?php
$db_host = "localhost";
$db_user = "root";
$db_passwd = "";
$db_name = "poker";
$conn = mysql_connect($db_host, $db_user, $db_passwd);
$db_id = mysql_select_db($db_name, $conn);

/*if ($db_id)
{
   echo "database 선택 성공";
}
else
{
   echo "databse 선택 실패";
}*/

$ins = mysql_query("INSERT INTO `user`(`id`, `pwd`, `email`, `nickname`, `money`, `win`, `lose`) VALUES ('" . $_POST['id'] . "','" . $_POST['pwd'] . "','" . $_POST['email'] . "','" . $_POST['nickName'] . "','100000000000','0','0');", $conn);
if ($ins){
	   
	   $data = array('rsponseMsg' => "join_succes");
	   echo json_encode( $data );
}	
else{
	   $data = array('rsponseMsg' => "join_fail");
	   echo json_encode( $data );
}
?>