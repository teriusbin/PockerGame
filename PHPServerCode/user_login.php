<?PHP

$db_host = "localhost";
$db_user = "root";
$db_passwd = "";
$db_name = "poker";

$id= $_POST['id'];
$pwd= $_POST['pwd'];

$conn = mysql_connect($db_host, $db_user, $db_passwd);
$db_id = mysql_select_db($db_name, $conn);
if ($db_id)
{
   //echo "database 선택 성공";
}
else
{
   //echo "databse 선택 실패";
}

$check = mysql_query("SELECT * FROM user WHERE `id`='".$id."'");
$numrows = mysql_num_rows($check);
if ($numrows == 0)
{
	$data = array('rsponseMsg' => noexist);
	echo json_encode( $data );
}
else
{
	while($row = mysql_fetch_assoc($check))
	{
		if ($pwd == $row["pwd"]){
			$query = "SELECT * FROM user WHERE `id` ='".$id."'"; 
			$result=mysql_query($query,$conn); 			
			 while($data = mysql_fetch_array($result)){ 
				  $jsonData = array(
							  'rsponseMsg' => "login_succses",
							  'money' => $data['money'],
							  'win' => $data['win'],
							  'lose'=>$data['lose']
				  );
			 }
			//$data = array('rsponseMsg' => "login_succses");
			echo json_encode( $jsonData );
		}
		else{
			$data = array('rsponseMsg' => "pwd_notMatch");
			echo json_encode( $data );
		}
	
	}

}

?>