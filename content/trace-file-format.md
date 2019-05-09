
## Notes
Prior to v2.6 xdebug simply appended destructor and shutdown function traces to the end of a trace file for a request. 
Since v2.6 it now creates separate files for each destructor and shutdown function trace, with the filename's first part being the same as the main trace.
If the xdebug.trace_options is set to 1, the separate trace files will be appended to the main file, with headers and everything. Parsing this kind of file is not currently supported.