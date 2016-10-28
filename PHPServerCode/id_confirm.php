<?php
$db_host = "localhost";
$db_user = "root";
$db_passwd = "";
$db_name = "poker";
$userid = $_POST['id'];

$conn = mysql_connect($db_host, $db_user, $db_passwd);
$db_id = mysql_select_db($db_name, $conn);
if (!$db_id)
{
	echo "databse 선택 실패";
}
$check = mysql_query("SELECT * FROM user WHERE `id`='".$userid."'");
$numrows = mysql_num_rows($check);
if ($numrows == 0)
{
	$data = array('rsponseMsg' => "confirm_ok");
	echo json_encode( $data );
}
else
{
	$data = array('rsponseMsg' => "confirm_no");
	echo json_encode( $data );
}

?>