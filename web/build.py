import sys, os, shutil
import markdown
from bs4 import BeautifulSoup

def set_article(soup, article, pre_process=None):
    with open(article, 'r', encoding='utf8') as f:
        text = f.read()

    html_text = markdown.markdown(text, extensions=['markdown.extensions.tables'])
    html = BeautifulSoup(html_text, features='lxml')
    if callable(pre_process):
        pre_process(html)

    articles = soup.find_all('article')
    if len(articles) == 0:
        print('ERROR: no article was found.')
        sys.exit(1)
    elif len(articles) > 1:
        print('WARN: multiple articles were found, use the first.')
    else:
        article = articles[0]
        article.clear()
        bodies = html.find_all('body')
        body = bodies[0]
        for i, child in enumerate(body.children):
            article.insert(i, child)

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
        elif 'active ' not in nav_item['class'] and i == index:
            nav_item['class'].append('active')

def migrate_img(soup, base_dir):
    images = soup.find_all('img')
    for image in images:
        src = image['src']
        src_file = os.path.join(base_dir, src)
        if os.path.exists(src_file):
            # local image found
            filename = os.path.basename(src_file)
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

    # witten manually

    # set_article(soup, os.path.join(script_folder, 'product.md'), 
    #     lambda soup: migrate_img(soup, script_folder))
    # delete_aside(soup)
    # delete_jumbotron(soup)
    # set_title(soup, '产品')
    # set_navbar_active(soup, 2)
    # with open('web/product.html', 'w', encoding='utf8') as f:
    #     f.write(soup.prettify())

    set_article(soup, os.path.join(script_folder, 'contact.md'))
    set_title(soup, '联系我们')
    set_navbar_active(soup, 3)
    with open('web/contact.html', 'w', encoding='utf8') as f:
        f.write(soup.prettify())