# J2JBreaker
## Overview
J2JBreaker is a J2J password estimator.

## Supported Formats
 * ZIP(except blank archive and spanned archive)
 * 7Z(Version 0.0~0.4)
 * RAR(v1.50 onwards, v5.00 onwards)

## Guide
### CLI Arguments
```
  J2JBreaker CLI Arguments Variables
  
  -i, --input           Required. Specifies a J2J file to estimate the password.
  -f, --format          Required. Specifies the file format(Available formats: ZIP, 7Z, RAR).
  -l, --length          (Default: 32) Specifies the max length of password.
  -t, --table           Specifies the path of a rainbow table file.
  -u, --use-specials    Specifies whether to use special characters.
  -n, --no-except       Specifies whether to except abnormal passwords.
  -b, --bruteforce      Sets the mode to brute-force mode.
  -s, --signature       Sets the mode to signature-based mode.
  -e, --use-enhancer    Specifies whether to use the enhancer
```

### Examples
```
Signature-Based Mode with the enhancer.
.\J2JBreaker.exe -i "C:\test.zip.J2J" -f zip -t ".\data\rt_common_words_3000.dat" -u -e -s
```

```
Brute-force Mode with 1,000,000 words.
.\J2JBreaker.exe -i "C:\test.7z.J2J" -f 7z -t ".\data\rt_10m_pw_list_1000000.dat" -u -b
```
