import os
import sys
import subprocess
from subprocess import PIPE

batch_path = os.path.expanduser(sys.argv[1])
working_dir = os.path.dirname(batch_path)

if not os.path.exists(batch_path):
    print("Batch file not found: " + batch_path)
    os.system('PAUSE')
    sys.exit(1)

"""
proc = subprocess.Popen(
    batch_path, shell=True,
    stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
"""
proc = subprocess.Popen(
    batch_path, shell=True, cwd=working_dir)

result = proc.communicate()
print(result)
