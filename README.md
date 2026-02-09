# Instructions

## Easy to use
1. `git clone` to any location
2. Hard link scripts to somewhere in a Unity project's `Assets` folder with `./symlink_all <source_dir> <target_dir>`.

### Notes
- Edit IP addresses manually
- Don't access the `transform` of the VR's `Camera` object (use the parent)
- Test with [my fork of LeRobot](https://github.com/ogoudey/lerobot) by running (from root) (outdated):
```python
import custom_brains.vla_interface as i
r = i.create_raw_teleop_mock(True) # True has the mock teleop use a default camera at `/dev/video0`
r.run()
# Turn on all Unity scripts.
``` 
