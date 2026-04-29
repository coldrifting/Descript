# -*- coding: utf-8 -*-
'''
To Use:
1. Adjust the global parameters in the first section below.
2. type `fontforge -script CreateFont.py` in the terminal.

Description:
This script uses FontForge to build a very basic monochrome font from a folder of SVG files.
Each SVG file must be named with the codepoint of the unicode character it is to be mapped to. 
    EG `1F94B.svg`
If a glyph maps to a sequence of codepoints, seperate the codepoints with `-` in the SVG file's name.
    EG `1F468-200D-1F9B3.svg`
If there are codepoints which are part of a sequence but lack their own SVG, then placeholder geometry is used.

See here for documentation about FontForge's scripting library:
https://fontforge.org/docs/scripting/python/fontforge.html

---

(c) 2022 Robert Winslow. Originally posted at https://github.com/RobertWinslow/Simple-SVG-to-Font-with-Fontforge
Feel free to use this little script however you like. I claim no ownership over any fonts you create with it.
All I ask is that you provide attribution by preserving this copyright notice in this script and its derivatives.
(If you need something formal, this script is released under a CC BY-SA 4.0 license.)
'''


#%% SECTION ONE - Imports and parameters
import fontforge
import os

INPUTFOLDER = 'Icons'
OUTPUTFILENAME = '../TunicRunes.ttf'
PLACEHOLDERGEOMETRYSVG = 'Icons/0020.svg'

font = fontforge.font()
font.familyname = "Tunic Runes"
font.fullname = font.familyname + " Regular"
font.copyright = "N/A"
font.version = "1.0"

# The following variables are for scaling the imported outlines.
SVGHEIGHT = 1000 # units of height of source svg viewbox.
GLYPHHEIGHT = 1000 # font units, default = 1000
PORTIONABOVEBASELINE = 0.8 # default is 0.8

# Width variables
SEPARATION = 140
SEPARATION_PUNCTUATION = 360
SPACEWIDTH = 240

#%% SECTION TWO A - Define function for importing outlines.

def importAndCleanOutlines(outlinefile,glyph):
    #print(outlinefile)
    glyph.importOutlines(outlinefile, simplify=True, correctdir=False, accuracy=0.25, scale=False)
    glyph.removeOverlap()
    SCALEFACTOR = GLYPHHEIGHT/SVGHEIGHT
    foregroundlayer = glyph.foreground
    for contour in foregroundlayer:
        for point in contour:
            point.transform((1,0,0,1,0,-800)) # Translate top of glyph down to baseline.
            point.transform((SCALEFACTOR,0,0,SCALEFACTOR,0,0)) # Scale up. Top of glyph will remain at baseline. 
            point.transform((1,0,0,1,0,PORTIONABOVEBASELINE*GLYPHHEIGHT)) # translate up to desired cap height
    glyph.setLayer(foregroundlayer,'Fore')

#%% SECTION TWO B - CREATE GLYPHS FROM THE SVG SOURCE FILES
# Scan the directory of SVG files and make a list of files and codepoints to process
files = os.listdir(INPUTFOLDER)
codetuples = [(tuple(filename[:-4].split('-')), filename) for filename in files if filename.endswith('.svg')]

# Start by loading up all the single codepoint characters.
simplecharacters = [(codepoints[0],filename) for codepoints,filename in codetuples if len(codepoints)==1]
for codepoint, filename in simplecharacters:
    char = font.createChar(int(codepoint,16), 'u'+codepoint)
    importAndCleanOutlines(INPUTFOLDER+'/'+filename,char)

# Manually add 200D (ZWJ), FE0F, and other individual codepoints as glyphs as needed.
# If a codepoint is not present as a glyph, we can't add it into a combined character.
# And if geometry isn't added to a glyph, FontForge will discard it.
# Therefore a placeholder glyph is used. 
presentcomponents = set([g.glyphname for g in font.glyphs()])
missingcodepoints = set()
for codepoints,filename in codetuples:
    for codepoint in codepoints:
        if 'u'+codepoint not in presentcomponents:
            missingcodepoints.add(codepoint)
for codepoint in missingcodepoints:
    char = font.createChar(int(codepoint,16), 'u'+codepoint)
    importAndCleanOutlines(PLACEHOLDERGEOMETRYSVG,char)


# Now make the combination characters via FontForge's ligature feature.
# To be quite honest, I don't fully understand what all this syntax up front is doing.
# Just treat these next couple of lines as if they are a mystical incantation.
font.addLookup('myLookup','gsub_ligature',None,(("liga",(('DFLT',("dflt")),)),))
font.addLookupSubtable("myLookup", "mySubtable")

combocharacters = [(codepoints,filename) for codepoints,filename in codetuples if len(codepoints)>1]

# Imports glyphs for all the non-skintone combination characters. 
for codepoints,filename in combocharacters:
    components = tuple('u'+codepoint for codepoint in codepoints)
    char = font.createChar(-1, '_'.join(components))
    char.addPosSub("mySubtable", components)
    importAndCleanOutlines(INPUTFOLDER+'/'+filename,char)

# Set Widths
font.selection.all()
font.autoWidth(-47)

# Adjust basic latin separately from runes
font.selection.select(("ranges", "unicode"), 0x0020, 0x007E)
font.autoWidth(SEPARATION)

# Make certain punctation  marks bigger to blend in with runes
font.selection.none()
font.selection.select('!', '.', ',')
font.autoWidth(SEPARATION_PUNCTUATION)

spaceChar = font.createChar(32, 'u0020')
spaceChar.width = SPACEWIDTH

#%% FINALLY - Generate the font
print("Generating black font to", OUTPUTFILENAME)
font.generate(OUTPUTFILENAME)








