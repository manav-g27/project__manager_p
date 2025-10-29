import { useEffect, useMemo, useState } from "react"; import { api } from "../api"; import { useParams } from "react-router-dom"; import dayjs from "dayjs";

type Task={id:number; title:string; dueDate?:string; isCompleted:boolean};
export default function Project(){
  const {id} = useParams();
  const [tasks,setTasks]=useState<Task[]>([]); const [t,setT]=useState(""); const [due,setDue]=useState<string>("");
  useEffect(()=>{ api.get(`/api/v1/projects/${id}/tasks`).then(r=>setTasks(r.data)); },[id]);
  async function add(){ const {data}=await api.post(`/api/v1/projects/${id}/tasks`,{title:t, dueDate: due? new Date(due): null}); setTasks(v=>[data,...v]); setT(""); setDue(""); }
  async function tog(i:number){ const {data}=await api.patch(`/api/v1/tasks/${i}/toggle`); setTasks(v=>v.map(x=>x.id===i?data:x)); }
  async function del(i:number){ await api.delete(`/api/v1/tasks/${i}`); setTasks(v=>v.filter(x=>x.id!==i)); }

  const cols = useMemo(()=>({ todo: tasks.filter(x=>!x.isCompleted), done: tasks.filter(x=>x.isCompleted) }),[tasks]);

  async function schedule(){
    const {data}=await api.post(`/api/v1/projects/${id}/schedule`,{startDate:new Date(), endDate: dayjs().add(6,'day').toDate()});
    alert("Schedule created for "+dayjs(data.start).format('DD MMM')+" → "+dayjs(data.end).format('DD MMM'));
    console.log(data);
  }

  return (
    <div className="p-4 max-w-5xl mx-auto space-y-4">
      <div className="nav p-3 flex items-center gap-2">
        <h1 className="text-2xl font-bold">Project #{id}</h1>
        <button className="ml-auto btn" onClick={schedule}>Smart Schedule</button>
      </div>
      <div className="card p-3 grid md:grid-cols-3 gap-2">
        <input className="input" placeholder="Task title" value={t} onChange={e=>setT(e.target.value)} />
        <input className="input" type="date" value={due} onChange={e=>setDue(e.target.value)} />
        <button className="btn" onClick={add}>Add Task</button>
      </div>

      <div className="kanban">
        <div className="kanCol">
          <h3 className="font-semibold mb-2">Active</h3>
          {cols.todo.map(x=> <Row key={x.id} t={x} onTog={()=>tog(x.id)} onDel={()=>del(x.id)} />)}
        </div>
        <div className="kanCol md:col-span-2">
          <h3 className="font-semibold mb-2">Completed</h3>
          {cols.done.map(x=> <Row key={x.id} t={x} onTog={()=>tog(x.id)} onDel={()=>del(x.id)} />)}
        </div>
      </div>

      <button className="fab" title="Add task" onClick={()=>document.querySelector<HTMLInputElement>("input.input")?.focus()}>＋</button>
    </div>
  );
}

function Row({t,onTog,onDel}:{t:Task; onTog:()=>void; onDel:()=>void}){
  const pct = (()=>{
    if(!t.dueDate) return 0.15; const d = (new Date(t.dueDate).getTime()-Date.now())/(1000*3600*24);
    return Math.max(0.05, Math.min(1, 1 - (d/30)));
  })();
  return (
    <div className="flex items-center gap-3 p-2 rounded-xl hover:bg-white/5">
      <div className="dueRing" style={{['--pct' as any]: pct}}/>
      <div className={"flex-1 "+(t.isCompleted?"line-through opacity-60":"")}>
        <div className="font-medium">{t.title}</div>
        {t.dueDate && <div className="text-xs opacity-60">Due {new Date(t.dueDate).toDateString()}</div>}
      </div>
      <button className="btn" onClick={onTog}>{t.isCompleted?"Uncomplete":"Complete"}</button>
      <button className="btn" onClick={onDel}>Delete</button>
    </div>
  );
}
