# Copyright (C) 2019-2020 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import os, re

docpath = os.path.dirname(__file__)
wikipath = os.path.abspath(os.path.join(docpath, '../../ligral.wiki'))
figurepath = 'https://gitee.com/junruoyu-zheng/ligral/raw/dev/doc/'
pagepath = 'https://junruoyu-zheng.gitee.io/ligral/'

def parsefilename(filewithext):
    res = filewithext.rsplit('.', maxsplit=1)
    if len(res) == 1:
        res.append('')
    return res

def gettitle(path, mdfile):
    with open(os.path.join(path, mdfile+'.md'), 'r', encoding='utf8') as f:
        head = f.readline()
    if head.startswith('#'):
        return head.lstrip('# ').rstrip()
    else:
        return translate.get(mdfile, mdfile)

def getfoldername(abspath, folder):
    if 'README.md' in os.listdir(abspath):
        return gettitle(abspath, 'README')
    else:
        return translate.get(folder, folder)

def copymdfile(srcfile, dstfile, refpath):
    with open(srcfile, 'r', encoding='utf8') as f:
        text = f.read()
    matches = re.findall('\[.*?\]\((.*?)\)', text)
    for match in matches:
        if not match.startswith('http') and not match.startswith('mail'):
            if parsefilename(match)[1] in ['png', 'gif', 'svg', 'jpg']:
                text = text.replace(match, figurepath+refpath.strip('./\\')+'/'+match.strip('/\\'))
            else:
                text = text.replace(match, pagepath+refpath.strip('./\\')+'/'+match.strip('/\\')+'.html')
    with open(dstfile, 'w', encoding='utf8') as f:
        f.write(text)

translate = {
    'api': '接口',
}

# path + folder = dir
def recursive_deploy(docfolder='.', wikifolder='.'):
    docdir = os.path.join(docpath, docfolder)
    wikidir = os.path.join(wikipath, wikifolder)
    for item in os.listdir(docdir):
        if item.startswith('.'):
            continue
        absdocitem = os.path.join(docdir, item)
        if os.path.isdir(absdocitem):
            print(f'enter folder {item}')
            wikiitem = getfoldername(absdocitem, item)
            abswikiitem = os.path.join(wikipath, wikifolder, wikiitem)
            newdocfolder = os.path.join(docfolder, item)
            newwikifolder = os.path.join(wikifolder, wikiitem)
            recursive_deploy(newdocfolder, newwikifolder)
        elif parsefilename(item)[1] == 'md':
            if not os.path.exists(wikidir):
                os.makedirs(wikidir)
                print(f'mkdir {wikidir}')
            wikifile = gettitle(docdir, parsefilename(item)[0])+'.md'
            absdocfile = os.path.join(docdir, item)
            abswikifile = os.path.join(wikidir, wikifile)
            copymdfile(absdocfile, abswikifile, docfolder)
            print(f'mv {absdocfile} {abswikifile}')


recursive_deploy()