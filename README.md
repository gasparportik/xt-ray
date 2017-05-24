## Xt-Ray
An xdebug trace file viewer

### About
This project aims at creating tools for viewing Xdebug call trace files(.xt).
It consists of a common core library(XtRay.Common / xtraylib) and several applications:

#### XtRay.Console: command line executable
Parse a trace file given as argument and dump it in various styles.
Planned features: 
 - parse file from standard input
 - apply filters to call tree
 - output back to standard xdebug trace format

#### XtRay.Windows: WPF GUI executable for Windows
Open a trace file and display it as expandable call tree with profile information.
Filter displayed call tree nodes by call name.
Planned features: 
 - more filters(call time, memory usage, is user-defined, etc.)
 - save filtered call tree to standard xdebug trace format
 - map trace file locations to local paths
 - show php code for selected call by loading local files
 - analyse call parameter usage and compare to local file function signature

#### XtRay.GtkSharp: GTK+ GUI executable for Linux (via Mono) and Windows
Should be able to do everything XtRay.Windows can. GUI is less customized.

### Contribute
Feel free to submit ideas and bugs to the [issues](https://github.com/boykathemad/xt-ray) of this repository.
Any code improvements are welcome, so please do fork and submit pull requests.  
