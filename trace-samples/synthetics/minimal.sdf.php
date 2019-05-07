<?php
register_shutdown_function(function () {
    time();
});
register_shutdown_function(function () {
    phpinfo();
});
