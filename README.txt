I. Introduction

WorkTimer is a program monitoring work time. It allows to displays periodic alerts and gather information about work time.


II. Interface

Program interface consists of 2 views:

1. Main view, which includes:

- Current work time (hh:mm) preview.

- Finish button  - allows finishes program and allows for saving information about work time.

- Save button - closes program and allows to continue with start time of last session when opened again.

- Stats button - shows statistics view.

- Config button - reloads program configuration form configuration files.

2. Statistics view, which includes:

- Stored work time information (beginning, ending, time, description).

- Export button - export work time information to csv file.

- OK button - closes this view. 


III. Configuration files

Program uses following configuration files (all with extension .wt):

1. Config - file with program configuration. Detailed desctiption in chapter IV.

2. Alerst - file contaning definition of alerts used in program. Detailed description of usage in chapter V.

3. Saved - file containing program start time. Should not be modified by user.

4. Stats - file containing statistics about work time. Should not be modified by user.

All above files are located in ConfigurationFiles folder in program working directory and are required for WorkTimer to work properly.


IV. Program configuration

WorkTimer can be configured by editing Config.wt configuration file.
Files consists of configauration variables definitions. Each entry has following structure:

ConfigurationVariableName=value

All empty lines and lines beginning with '#' sign are ignored. 
Program can be configured by replacing text after '=' sign in line concerning chosen setting.

Settings description (Setting number. Connected configuration variable [value type] - description):

1. FinishAdditionalInfoEnabled ['true' or 'false'] - if 'true' enables additional timed events information showing with work finish message, depending on current time.

2. FinishAdditionalInfoNumber [positive integer] - number of events presented on program finish. Works only with [1] set to 'true'. 

3. FinishAdditionalInfoHeader [text] -  header ontop of the events info. Works only with [1] set to 'true'. 

4. FinishAdditionalInfoFile=ConfigurationFiles\BusInfoMS.wt - file containing definition of events in separate lines, ordered by time. Each line must have following structure: hour(in format hh:mm):Event text. Works only with [1] set to 'true'.

5. StatsEnables ['true' or 'false'] - if 'true' enables gathering statistics about work time.

6. StatsBeginningsHeader [text] -  header for column with work beginning time in statistics export file.

7. StatsEndingsHeader [text] -  header for column with work ending time in statistics export file.

8. StatsTimeHeader [text] -  header for column with work duration in statistics export file.

9. StatsCommentsHeader [text] -  header for column with work description in statistics export file.

10. StatsSumHeader [text] - header for sum of work time.

11. StatsAverageHeader [text] - header for average work time.

12. DefaultWorkTime [positive integer] - default work time, after which finishing message will be shown.

13. BalanceOn ['true' or 'false'] - enables balancing work time. If set to 'true' time of displaying next finishing message may be changed in order to achieve average work time equal to default work time.

14. BalanceDays [positive integer] - indicates on how many days time balancing will be spread.

15. OnFinishMessage [text] - text of finishing message.

16. SummariesEnabled ['true' or 'false'] - if set to 'true' enables saving work descriptions after finishing work.

17. VisibleInTaskbar ['true' or 'false'] - if set to 'true' program is visible in taskbar.


V. Alerts configuration

WorkTimer alerts are defined in Alerts.wt configuration file.
Files consists of additional alerts definitions in separate lines. Each consists of 4 values separated by tabulation character:
1. Is alert active or not - can be only 'true' or 'false'.
2. Time after which alert will be shown (if active) - can be olny positive integer number.
3. Determines if alert will be repeated after first period expires - can be only 'true' or 'false'.
4. Contains alert message text - can be any text (not containing tabulations).

First line in file is ignored. 
Alerts definitions are ordered in ascending priority.