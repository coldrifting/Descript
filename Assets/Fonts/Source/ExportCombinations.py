#!/usr/bin/env python3

import re

def processLines(template: str, ids: list[int]) -> str:
    newLines : list[str] = []

    pattern = r'id="s(\d+)'

    for line in template.split("\n"):
        matches = re.findall(pattern, line)

        if (len(matches) == 0):
            newLines.append(line)
            continue

        currentId: int = int(matches[0])
        if (currentId in ids):
            newLines.append(line)

    return "\n".join(newLines)

def getIntArray(num: int) -> list[int]:
    result: list[int] = []

    for i in range(0,15):
        flag: int = 1 << i
        if (num & flag) == flag:
            result.append(i)

    return result

with open("RuneTemplate.svg","r") as templateFile:
    template : str = templateFile.read()

    for i in range(0,4096):
        arr: list[int] = getIntArray(i)
        newLines = processLines(template, arr)
        newLines = newLines.replace('id="rune"', f'id="{i}"')
        num: int = 0xE000 + i
        with open(f'Icons/{num:04X}.svg',"w") as outputFile:
            outputFile.write(newLines)
            outputFile.close()

    templateFile.close()