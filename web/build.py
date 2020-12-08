import sys, os, shutil
import markdown
from bs4 import BeautifulSoup, element

def set_article(soup, article, pre_process=None):
    if isinstance(article, str):
        with open(article, 'r', encoding='utf8') as f:
            text = f.read()

        html_text = markdown.markdown(text, extensions=['markdown.extensions.tables'])
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
            a = soup.new_tag('a', **{'class':'list-group-item'}, href=val+'.html')
            a.append(soup.new_string(key))
            root.append(a)
        elif isinstance(val, dict):
            a = soup.new_tag('a', **{'class':'list-group-item'}, href=val[list(val)[0]]+'.html')
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

if __name__ == "__main__":
    script_folder = os.path.dirname(__file__)
        
    with open('web/index.html', 'r', encoding='utf8') as f:
        text = f.read()

    soup = BeautifulSoup(text, features='lxml')

    set_article(soup, os.path.join(script_folder, '../README.md'))
    with open('web/index.html', 'w', encoding='utf8') as f:
        f.write(soup.prettify())

    print('INFO: index.html done.')

    move_aside(soup)
    set_aside(soup, {
        '快速开始': 'quick-start',
        '用户文档': {
            '术语定义': 'user-guide/terms'
            '基础语法': 'user-guide/basic-grammar',
            '矩阵运算': 'user-guide/matrix-cal',
            '路由类型': 'user-guide/route',
            '继承关系': 'user-guide/inherit'
        },
        '开发文档': {
            '语法解析': 'dev-guide/interpreter',
            '模块组件': 'dev-guide/model-component',
            '问题抽象': 'dev-guide/formulation'
        },
        '接口定义': {
            '模块接口': 'interface/model',
            '数据接口': 'interface/protocol'
        }
    })
    set_article(soup, os.path.join(script_folder, '../doc/quick-start/README.md'))
    delete_jumbotron(soup)
    set_title(soup, '快速开始')
    set_navbar_active(soup, 1)
    set_item_active(soup, 0)
    migrate_img(soup, os.path.join(script_folder, '../doc/quick-start'))
    with open('web/quick-start.html', 'w', encoding='utf8') as f:
        f.write(soup.prettify())

    print('INFO: quick-start.html done.')

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