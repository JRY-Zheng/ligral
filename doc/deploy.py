import os, shutil

docpath = os.path.dirname(__file__)
wikipath = os.path.abspath(os.path.join(docpath, '../../ligral.wiki'))

def parsefilename(filewithext):
    return filewithext.rsplit('.', maxsplit=1)

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
            shutil.copy(absdocfile, abswikifile)
            print(f'mv {absdocfile} {abswikifile}')


recursive_deploy()