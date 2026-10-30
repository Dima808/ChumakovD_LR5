"""Реальна інтеграційна перевірка запущеного API без сторонніх Python-пакетів."""
import json,sys,urllib.request,urllib.error
from pathlib import Path

base=sys.argv[1] if len(sys.argv)>1 else 'http://localhost:5085'
root=Path(__file__).resolve().parents[1]
checks=[];requests=[]
def call(method,path,data=None):
 req=urllib.request.Request(base+path,data=None if data is None else json.dumps(data,ensure_ascii=False).encode(),headers={'Content-Type':'application/json'},method=method)
 try:response=urllib.request.urlopen(req)
 except urllib.error.HTTPError as e:response=e
 raw=response.read().decode();body=json.loads(raw) if raw else None
 requests.append(dict(method=method,path=path,request=data,status=response.status,response=body,headers=dict(response.headers)))
 return response.status,body,response.headers
def check(name,condition):
 assert condition,name
 checks.append(name);print('PASS',name)
sample=json.loads((root/'src/data/todo-single.json').read_text())
status,item,headers=call('POST','/api/TodoItems',dict(sample,secret='must-not-be-exposed'))
check('POST 201, серверний Id, Location, DueDate і Priority',status==201 and item['id']>0 and headers.get('Location','').endswith('/'+str(item['id'])) and item['dueDate']==sample['dueDate'] and item['priority']==3)
id1=item['id'];check('DTO не повертає Secret','secret' not in item)
status,body,_=call('GET',f'/api/TodoItems/{id1}');check('GET одного запису 200',status==200 and body==item)
status,_,_=call('GET','/api/TodoItems/9223372036854775807');check('GET відсутнього запису 404',status==404)
batch=json.loads((root/'src/data/todo-batch.json').read_text())
status,created,_=call('POST','/api/TodoItems/batch',batch)
check('POST batch 201 і три повні об’єкти з унікальними Id',status==201 and len(created)==3 and len({x['id'] for x in created})==3 and all(set(x)=={'id','name','isComplete','dueDate','priority'} for x in created))
id2=created[0]['id']
status,found,_=call('GET',f'/api/TodoItems/by-ids?ids={id1}&ids={id2}&ids=9223372036854775807')
check('Пошук кількох Id повертає лише знайдені записи',status==200 and {x['id'] for x in found}=={id1,id2})
for query in [f'ids={id1}',f'ids={id1}&ids={id1}','ids=-1&ids=2','']:
 status,_,_=call('GET','/api/TodoItems/by-ids?'+query);check('Пошук відхиляє некоректні Id '+query,status==400)
updated=dict(item,name='Звіт перевірено',isComplete=True,dueDate='2026-11-08',priority=4)
status,_,_=call('PUT',f'/api/TodoItems/{id1}',updated);check('PUT повертає 204',status==204)
status,body,_=call('GET',f'/api/TodoItems/{id1}');check('PUT зберіг DueDate і Priority',status==200 and body==updated)
status,_,_=call('PUT',f'/api/TodoItems/{id1}',dict(updated,id=id1+10000));check('PUT перевіряє збіг Id',status==400)
for title,data in [('неправильний пріоритет',dict(sample,priority=0)),('порожня назва',dict(sample,name='')),('відсутня дата',{k:v for k,v in sample.items() if k!='dueDate'}),('неправильна дата',dict(sample,dueDate='bad')),('клієнтський Id',dict(sample,id=9000))]:
 status,_,_=call('POST','/api/TodoItems',data);check('POST відхиляє '+title,status==400)
for title,data in [('порожній масив',[]),('null замість масиву',None),('null елемент',[None]),('клієнтський Id',[dict(sample,id=9000)]),('невалідний елемент',[sample,dict(sample,priority=0)]),('понад 200 елементів',[sample]*201)]:
 _,before,_=call('GET','/api/TodoItems')
 status,_,_=call('POST','/api/TodoItems/batch',data)
 _,after,_=call('GET','/api/TodoItems')
 check('Batch відхиляє '+title+' без часткового запису',status==400 and before==after)
dataset=json.loads((root/'src/data/todos.json').read_text())
status,many,_=call('POST','/api/TodoItems/batch',dataset);check('Імпорт 120 логічних JSON-записів',status==201 and len(many)==120 and len({x['id'] for x in many})==120)
for x in many:call('DELETE',f"/api/TodoItems/{x['id']}")
status,_,_=call('DELETE',f'/api/TodoItems/{id1}');check('DELETE повертає 204',status==204)
status,_,_=call('GET',f'/api/TodoItems/{id1}');check('Після DELETE запис відсутній',status==404)
status,_,_=call('DELETE',f'/api/TodoItems/{id1}');check('Повторний DELETE повертає 404',status==404)
for x in created:call('DELETE',f"/api/TodoItems/{x['id']}")
out=root/'Documentation';out.mkdir(exist_ok=True)
(out/'api-tests.json').write_text(json.dumps({'passed':len(checks),'checks':checks,'requests':requests},ensure_ascii=False,indent=2),encoding='utf8')
print(f'RESULT {len(checks)} passed, 0 failed')
