#!/usr/bin/env python3
import os
import re
import shutil
from pathlib import Path


def process_lines(template: str, ids: list[int]) -> str:
    newLines : list[str] = []

    pattern = r'id="s(\d+)'

    for line in template.split("\n"):
        matches = re.findall(pattern, line)

        if len(matches) == 0:
            newLines.append(line)
            continue

        current_id: int = int(matches[0])
        if current_id in ids:
            newLines.append(line)

    return "\n".join(newLines)

def get_int_array(num: int) -> list[int]:
    result: list[int] = []

    for i in range(0,15):
        flag: int = 1 << i
        if (num & flag) == flag:
            result.append(i)

    return result

Path("Icons").mkdir(exist_ok=True)

directory = os.fsencode("Ascii")
for file in os.listdir(directory):
    filename = os.fsdecode(file)
    if filename.endswith(".svg"): 
        source = os.path.join("Ascii", filename)
        dest = os.path.join("Icons", filename)
        shutil.copy(source, dest)
    else:
        continue
        
with open("RuneTemplate.svg","r") as templateFile:
    template : str = templateFile.read()

    for i in range(0,4096):
        arr: list[int] = get_int_array(i)
        newLines = process_lines(template, arr)
        newLines = newLines.replace('id="rune"', f'id="{i}"')
        num: int = 0xE000 + i
        with open(f'Icons/{num:04X}.svg',"w") as outputFile:
            outputFile.write(newLines)
            outputFile.close()

    templateFile.close()
