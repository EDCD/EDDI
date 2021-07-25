# User Lexicons

Many voices support user customized voice pronunciations via lexicon files. The method for generating lexicons file differs for different kinds of [voices](##Voices) and different [phonetic alphabets](##Alphabets).

## Voices

### Standard voices

If you have selected a voice which supports it (including many Microsoft voices), you can create a `lexicons` directory inside of your configuration directory (e.g. `%appdata%/EDDI/lexicons`) and add a [.pls formatted lexicon file](https://www.w3.org/TR/pronunciation-lexicon/) to create your own customizations to EDDI's pronuncations. 

Each culture (or accent) should have a different lexicon. Please name the file to match the culture code of the voice or voices you'd like to match and to match the value of the \"xml:lang\" attribute of the .pls file, e.g. \"en-gb.pls\" for British voices or \"en-us.pls\" for United States voices. You can also use a two letter culture code (e.g. \"en.pls\"), though the more specific language codes will be preferred.

Once you are done editing the .pls file, please restart EDDI to allow the changes to take effect.

### Ivona voices

Ivona voice installers include a Lexicon Editor utility. The utility can be used to change word pronunciations, and includes support for phonemes. The lexicon editor utility supports SSML tags only, meaning SAPI5 phonemes are not supported.  Either the [IPA](###IPA) or [X-SAMPA](###X-SAMPA) can be used to create phoneme pronunciations in SSML. 

To access the Lexicon Editor utility, from the Control Panel navigate to `Ease of Access` then `Speech Recognition` then `Advanced speech options`. 

![](../images/lexicons-Ivona1.png)

Open the Text to Speech tab, make sure that your Ivona voice is selected, then hit the `Settings` button to open the Lexicon Editor.

![](../images/lexicons-Ivona2.png)

To use phonemes in the Lexicon Editor, you need to translate the word you want to correct to an SSML phoneme tag using the [IPA](###IPA) or [X-SAMPA](###X-SAMPA) alphabets. 

![](../images/lexicons-Ivona3.png)

Notes:
- changes take effect immediately (you do not need to restart EDDI)
- changes match on partial words so adjusting the pronunciation of "xeno" will also influence the pronunciation of words like "xenobiology" or "anti-xeno".

### Cereproc voices

CereProc SAPI5 voices support the addition of a user lexicon as follows:

- Navigate to the Cereproc voice file. If you've installed to the default directory, this should be `C:\Program Files (x86)\CereProc\CereVoice [voice] 6.0.0\` (where `[voice]` is the specific name of the Cereproc voice you have installed, e.g. Heather, Caitlin, etc.).
- In the same directory as the voice file, add a new text file called `user_lexicon.txt`.
- Open the file and add new lexicon entries (or override existing ones) by adding a *headword* and *pronunciation*, one per line, separated by a single space, e.g.
     >test v_oi1_s
- The *headword* should be lower case, and consist of alphabetic characters only (to process a string that includes non-alphabetic characters, use an [abbreviations file](https://www.cereproc.com/files/CereVoiceCloudGuide.pdf#%5B%7B%22num%22%3A305%2C%22gen%22%3A0%7D%2C%7B%22name%22%3A%22XYZ%22%7D%2C0%2C656%2C0%5D))
- The *pronunciation* may either use the [IPA](###IPA) or [Cereproc](###Cereproc) alphabets.
- When you are done editing, save the file and then restart EDDI or any other Text to Speech applications to allow the changes to take effect.

## Alphabets

### IPA

IPA is the International Phonetic Alphabet. The IPA alphabet includes non-ASCII symbols which must be pasted into the program rather than typed, though many online dictionaries provide IPA pronunciations which can readily be pasted into any lexicon.
 
Here are examples of the word “xeno” using IPA:

- ˈzinoʊ
- ˈzinə
- ˈzɛnə
- ˈzɛnoʊ

### Cereproc

The Cereproc alphabet is documented [here](https://www.cereproc.com/files/CereVoicePhoneSets.pdf) and is used in a lexicon as described [here](https://www.cereproc.com/files/CereVoiceCloudGuide.pdf#%5B%7B%22num%22%3A303%2C%22gen%22%3A0%7D%2C%7B%22name%22%3A%22XYZ%22%7D%2C0%2C804%2C0%5D).

This example will change the pronunciation of the word 'test' to 'voice':

>test v_oi1_s

All vowel phenomes require a stress marker:

1 = primary stress  
2 = secondary stress  
0 = no stress

> plane p_l_ei1_n  
> planeload p_l_ei1_n_l_ou2_d  
> voices v_oi1_s_i0_z

If words with accented characters are added, the encoding must be UTF-8.

### X-SAMPA

X-SAMPA is a computer-readable phonetic script using 7-bit printable ASCII characters, based on the International Phonetic Alphabet (IPA). X-SAMPA was devised at a time when Unicode text encoding formats did not support IPA symbols but is typically no longer necessary since Unicode support for IPA symbols has become much more widespread. For more details on the X-SAMPA alphabet, please see [here](https://en.wikipedia.org/wiki/X-SAMPA).
