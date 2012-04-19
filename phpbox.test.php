<?php

interface phpbox_interface
{

	function status($data);

	function caption($data);

	function disableStart();

	function disableStop();

	function disableFile();

	function disableParameter();

	function doExit();

	function lines($int=1);

	function progress($intfloat=0);

	function notice($string);

	function error($string);

	function delay($factor);

	function stopScript($file);
}

class phpbox implements phpbox_interface
{

	public $debugMode = true;
	public $sendDelay = 350000;

	public function stopScript($file)
	{
		if( !file_exists($file) ) $this->notice("Missing STOP_SCRIPT!!");
		$this->send("STOP_SCRIPT", $file);
	}

	public function disableStart()
	{
		$this->send("DISABLE_START");
	}

	public function disableStop()
	{
		$this->send("DISABLE_STOP");
	}

	public function disableFile()
	{
		$this->send("DISABLE_FILE");
	}

	public function disableParameter()
	{
		$this->send("DISABLE_PARAMETER");
	}

	public function doExit()
	{
		$this->send("DO_EXIT");
	}

	public function lines($int=1)
	{
		$store = $this->sendDelay;
		$this->sendDelay *= 0.1;
		$this->send("LINES", $int);
		$this->sendDelay = $store;
	}

	public function progress($intfloat=0)
	{
		$this->send("PROGRESS", $intfloat);
	}

	public function notice($string)
	{
		$this->send("NOTICE", $string);
	}

	public function error($string)
	{
		$this->send("ERROR", $string);
	}

	public function caption($data)
	{
		$this->send("CAPTION", $data);
	}

	public function iterate($cmd, $repeat, $arg1=NULL, $arg2=NULL)
	{
		if( method_exists($this, $cmd) )
		{
			for( $x = 1; $x <= $repeat; $x++ )
			{
				if( isset($arg1) ) $argument1 = sprintf($arg1, $x, $repeat);
				if( isset($arg2) ) $argument2 = sprintf($arg2, $x, $repeat);

				if( isset($arg1) && isset($arg2) )
				{
					$this->{$cmd}($argument1, $argument2);
				}
				elseif( isset($arg1) && !isset($arg2) )
				{
					$this->{$cmd}($argument1);
				}
				elseif( !isset($arg1) && !isset($arg2) )
				{
					$this->{$cmd}();
				}
			}
		}
	}

	public function status($data)
	{
		$this->send("STATUS", $data);
	}

	private function send($cmd, $data=NULL)
	{
		$cmd = strtoupper($cmd);
		$msg = '{'.$cmd.(isset($data) ? '|'.$data : '').'}';
		$this->debug_send("SEND ".$msg);
		$this->push($msg."\n");
		$this->delay(0.3);
		$this->debug_send("DONE!");
		$this->delay();
	}

	private function push($string)
	{
		echo $string;
	}

	private function debug_send($msg)
	{
		if( !$this->debugMode ) return;

		$dbg = "DEBUG: ".$msg;
		$this->push($dbg."\n");
	}

	public function delay($factor=1)
	{
		usleep($this->sendDelay * $factor);
	}

}

$test = new phpbox;

//$test->doExit();
//$test->stopScript("clean.php");

$test->status("This is a STATIS-command test!");
//$test->notice("Start STATUS serial test!");
$test->iterate("status", 5, "STATUS-command test %01d of %01d");

$test->caption("This is a CAPTION-command test!");
$test->iterate("caption", 5, "CAPTION-command test %01d of %01d");

$test->lines(8);

$test->progress(50);
$test->delay(2);
$test->progress(75);
$test->delay(2);
$test->progress(25);
$test->delay(2);
$test->progress(100);
$test->delay(2);
$test->progress(0);
$test->delay(2);

$test->progress(0.2);
$test->delay(2);
$test->progress(0.4);
$test->delay(2);
$test->progress(0.6);
$test->delay(2);
$test->progress(0.8);
$test->delay(2);

$test->lines(100);
//$test->notice("Starte dynamic resize!");
$test->delay(10);
$test->iterate("lines", 100, "%d");
/*
$test->disableStop();
$test->disableStart();
$test->disableParameter();
$test->disableFile();

$test->error("Das ist ein Testfehler");
*/
?>