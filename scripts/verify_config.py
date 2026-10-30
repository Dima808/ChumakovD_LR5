import json,subprocess,tempfile,os,sys,shutil
from pathlib import Path
root=Path(__file__).resolve().parents[1]
sdk=sys.argv[1] if len(sys.argv)>1 else shutil.which('dotnet')
assert sdk, '.NET SDK 8 не знайдено у PATH'
results=[]
for name,value in [('null',None),('empty',''),('short','abc'),('spaces','    ')]:
 with tempfile.TemporaryDirectory() as folder:
  f=Path(folder).resolve();(f/'appsettings.json').write_text(json.dumps({'Database':{'Name':value}}))
  env=dict(os.environ,ASPNETCORE_ENVIRONMENT='Development')
  run=subprocess.run([sdk,str(root/'bin/Debug/net8.0/ChumakovD_LR5.dll'),'--contentRoot',str(f),'--urls','http://localhost:5086'],env=env,text=True,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=20)
  lines=[line.strip() for line in run.stdout.splitlines() if 'Database:Name' in line or 'OptionsValidationException' in line]
  expected='щонайменше 4' if name=='short' else 'не може бути null'
  assert run.returncode!=0 and expected in run.stdout and 'Now listening on:' not in run.stdout,(name,run.stdout)
  results.append(dict(case=name,value=value,exitCode=run.returncode,error=lines[0],passed=True))
  print(f'PASS Database:Name={json.dumps(value)} -> старт відхилено')
  print(lines[0])
(root/'Documentation/config-tests.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf8')
print('RESULT 4 passed, 0 failed')
