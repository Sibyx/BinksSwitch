## 0.4.0 : 01.12.2019

- **Change**: Firewall redesigned to first-fit
- **Change**: Firewall rule checking using validation stack (`Stack<Func<bool>>`)
- **Change**: Source and destination port is now nullable because of [ICMP Reply messsage](https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol)

## 0.3.0 : 21.11.2019

- **Feature**: Firewall

## 0.2.0 : 19.11.2019

- **Feature**: Change settings using `SettingsWindow`
- **Feature**: Logging using [Syslog](https://tools.ietf.org/html/rfc5424)
- **Feature**: Statistics

## 0.1.0 : 3.11.2019

- **Feature**: CAM Table using `ConcurrentDictionary`
- **Feature**: Basic switching functionality
- **Feature**: Loading settings from `Properties.Settings.Default`