<?php
class Hello {
    public function __construct() {
        echo 'constructed';
    }
    public function __destruct() {
        echo 'destructed';
    }
}
$x = new Hello();
