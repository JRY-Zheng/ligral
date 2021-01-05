# Copyright 2019-2020 Junruoyu Zheng. All rights reserved.

# Distributed under MIT license.
# See file LICENSE for detail or copy at https://opensource.org/licenses/MIT


import sys, os, shutil, re
import markdown
from bs4 import BeautifulSoup, element

def set_article(soup, article, pre_process=None):
    if isinstance(article, str):
        if os.path.exists(article):
            with open(article, 'r', encoding='utf8') as f:
            #     text = f.read()
                lines = f.readlines()
        else:
            print(f'{article} not found')
            lines = ['# 我还没写呢，过段时间再来看吧~']
        
        for i, line in enumerate(lines):
            if not line.startswith('    '):
                lines[i], _ = re.subn(r'\$([^$\n]+)\$', '$`\g<1>`$', line)

        text = ''.join(lines)

        # text, num =  re.subn(r'\$([^$\n]+)\$', '$`\g<1>`$', text)

        html_text = markdown.markdown(text, 
            extensions=[
                'markdown.extensions.tables', 
                'markdown_katex'
            ],
            extension_configs={
                'markdown_katex': {
                    'no_inline_svg': True,      # fix for WeasyPrint
                    'insert_fonts_css': True,
                },
            }
        )
        html = BeautifulSoup(html_text, features='lxml')
    elif isinstance(article, element.Tag):
        html = article
    else:
        raise ValueError(f'article should be md filename or html object, but {type(article)}')

    if callable(pre_process):
        pre_process(html)

    articles = soup.find_all('article')
    if len(articles) == 0:
        raise ValueError('ERROR: no article was found.')
    elif len(articles) > 1:
        print('WARN: multiple articles were found, use the first.')
    else:
        article = articles[0]
        article.clear()
        links = html.find_all('a')
        for link in links:
            if link['href'].startswith('http'):
                link['target'] = '_blank'
        bodies = html.find_all('body')
        if bodies:
            body = bodies[0]
            for i, child in enumerate(body.children):
                article.insert(i, child)
        else:
            article.replace_with(html)

def move_aside(soup):
    dives = soup.find_all('div', class_='row')
    div = dives[1]
    div['style'] = 'flex-direction: row-reverse;'

def recursive_cat(root, cat):
    for key, val in cat.items():
        if isinstance(val, str):
            a = soup.new_tag('a', **{'class':'list-group-item'}, href='/ligral/'+val+'.html')
            a.append(soup.new_string(key))
            root.append(a)
        elif isinstance(val, dict):
            folder = val[list(val)[0]].rsplit('/', maxsplit=1)[0]
            a = soup.new_tag('a', **{'class':'list-group-item'}, href='/ligral/'+folder+'/index.html')
            a.append(soup.new_string(key))
            root.append(a)
            box = soup.new_tag('div', **{'class':'list-group'})
            root.append(box)
            recursive_cat(box, val)
        else:
            raise ValueError(f'item should be str or dict but {val}')

def set_aside(soup, cat):
    asides = soup.find_all('aside')
    aside = asides[0]
    boxes = aside.find_all('div', class_='sidebar-box')
    if not boxes:
        raise ValueError('no cat box')
    box = boxes[0]
    box.clear()
    h4 = soup.new_tag('h4')
    h4.append(soup.new_string('目录'))
    box.append(h4)
    root = soup.new_tag('div', **{'class':'list-group list-group-root'})
    box.append(root)
    recursive_cat(root, cat)

def delete_aside(soup):
    asides = soup.find_all('aside')
    for aside in asides:
        aside.extract()
    div = soup.find('div', class_='col-sm-8')
    div['class'].remove('col-sm-8')
    div['class'].append('col')

def delete_jumbotron(soup):
    jumbotrons = soup.find_all('div', class_='jumbotron-wrap')
    for jumbotron in jumbotrons:
        jumbotron.extract()

def set_title(soup, title):
    title_element = soup.find('title')
    title = soup.new_string(title)
    title_element.clear()
    title_element.insert(0, title)

def set_navbar_active(soup, index):
    nav_items = soup.find_all('li', class_='nav-item')
    for i, nav_item in enumerate(nav_items):
        if 'active' in nav_item['class'] and i != index:
            nav_item['class'].remove('active')
        elif 'active' not in nav_item['class'] and i == index:
            nav_item['class'].append('active')

def set_item_active(soup, index):
    items = soup.find_all('a', class_='list-group-item')
    for i, item in enumerate(items):
        if 'active' in item['class'] and i != index:
            item['class'].remove('active')
        elif 'active' not in item['class'] and i == index:
            item['class'].append('active')

def migrate_img(soup, base_dir):
    images = soup.find_all('img')
    for image in images:
        if image['alt'].endswith('!!'):
            image['style'] = 'max-width:100%;'
        src = image['src']
        src_file = os.path.join(base_dir, src)
        if os.path.exists(src_file):
            # local image found
            filename = os.path.basename(src_file)
            print('found local', filename)
            dst_file = f'web/img/{filename}'
            if os.path.exists(dst_file):
                print(f'{dst_file} already exists, will be overrode.')
            try:
                shutil.copy(src_file, dst_file)
            except shutil.SameFileError:
                print('same file, ignored')
            image['src'] = f'img/{os.path.basename(src_file)}'

def deploy_docs(conf, index, head=None):
    titles = list(conf)
    if head:
        titles.remove(head)
        titles.insert(0, head)
    for title in titles:
        if isinstance(conf[title], str):
            path = conf[title]
            if os.path.isdir('doc/' + path):
                if os.path.isdir('web/' + path):
                    html = path + '/index.html'
                    print(f'DEBUG: web/{path} found')
                else:
                    html = path + '.html'
                    print(f'DEBUG: web/{path} not found')
                md = path + '/README.md'
            else:
                html = path + '.html'
                md = path + '.md'
            # print(f'DEBUG: {md} found')
            set_article(soup, os.path.join(script_folder, '../doc/' + md))
            set_title(soup, title)
            set_item_active(soup, index)
            index += 1
            migrate_img(soup, os.path.join(script_folder, '../doc/' + md.rsplit('/')[0]))
            with open('web/' + html, 'w', encoding='utf8') as f:
                f.write(soup.prettify())

            print(f'INFO: {html} done.')
        elif isinstance(conf[title], dict):
            folder = conf[title]
            folder[title] = folder[list(folder)[0]].rsplit('/')[0]
            if not os.path.isdir('web/' + folder[title]):
                os.mkdir('web/' + folder[title])
            index = deploy_docs(folder, index, title)

    return index

if __name__ == "__main__":
    script_folder = os.path.dirname(__file__)
        
    with open('web/index.html', 'r', encoding='utf8') as f:
        text = f.read()

    soup = BeautifulSoup(text, features='lxml')

    set_article(soup, os.path.join(script_folder, '../README.md'))
    migrate_img(soup, os.path.join(script_folder, '..'))
    links = soup.find_all('a')
    for link in links:
        if link['href'].startswith('http'):
            link['target'] = '_blank'
    with open('web/index.html', 'w', encoding='utf8') as f:
        f.write(soup.prettify())

    print('INFO: index.html done.')

    move_aside(soup)
    conf = {
        '快速开始': 'quick-start',
        '用户文档': {
            '术语定义': 'user-guide/terms',
            '设置语句': 'user-guide/config',
            '声明常量': 'user-guide/const',
            '声明节点': 'user-guide/node',
            '节点连接': 'user-guide/link',
            '矩阵运算': 'user-guide/matrix',
            '路由类型': 'user-guide/route',
            '接口签名': 'user-guide/signature',
            '引用依赖': 'user-guide/import',
        },
        '开发文档': {
            '语法解析': 'dev-guide/syntax',
            '模块组件': 'dev-guide/component',
            '问题抽象': 'dev-guide/simulation',
            '工具箱': 'dev-guide/tools'
        },
        '接口定义': {
            '模块接口': 'interface/model',
            '数据接口': 'interface/protocol'
        }
    }
    set_aside(soup, conf)

    delete_jumbotron(soup)
    set_navbar_active(soup, 1)
    deploy_docs(conf, 0)
    # set_article(soup, os.path.join(script_folder, '../doc/quick-start/README.md'))
    # set_title(soup, '快速开始')
    # set_item_active(soup, 0)
    # migrate_img(soup, os.path.join(script_folder, '../doc/quick-start'))
    # with open('web/quick-start.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: quick-start.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/README.md'))
    # set_title(soup, '用户文档')
    # set_item_active(soup, 1)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/index.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/index.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/terms.md'))
    # set_title(soup, '术语定义')
    # set_item_active(soup, 2)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/terms.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/terms.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/config.md'))
    # set_title(soup, '设置语句')
    # set_item_active(soup, 3)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/config.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/config.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/const.md'))
    # set_title(soup, '声明常量')
    # set_item_active(soup, 4)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/const.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/const.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/node.md'))
    # set_title(soup, '声明节点')
    # set_item_active(soup, 5)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/node.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/node.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/link.md'))
    # set_title(soup, '节点连接')
    # set_item_active(soup, 6)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/link.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/link.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/matrix.md'))
    # set_title(soup, '矩阵计算')
    # set_item_active(soup, 6)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/matrix.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/matrix.html done.')

    # set_article(soup, os.path.join(script_folder, '../doc/user-guide/route.md'))
    # set_title(soup, '路由类型')
    # set_item_active(soup, 6)
    # migrate_img(soup, os.path.join(script_folder, '../doc/user-guide'))
    # with open('web/user-guide/route.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    # print('INFO: user-guide/route.html done.')

    with open(os.path.join(script_folder, 'product.html'), 'r', encoding='utf8') as f:
        prod_text = f.read()
    prod_soup = BeautifulSoup(prod_text, features='lxml')
    set_article(soup, prod_soup.find_all('article')[0])
    delete_aside(soup)
    set_title(soup, '产品')
    set_navbar_active(soup, 2)
    with open('web/product.html', 'w', encoding='utf8') as f:
        f.write(soup.prettify())

    print('INFO: product.html done.')

    set_article(soup, os.path.join(script_folder, 'contact.md'))
    set_title(soup, '联系我们')
    set_navbar_active(soup, 3)
    with open('web/contact.html', 'w', encoding='utf8') as f:
        f.write(soup.prettify())

    print('INFO: contact.html done.')