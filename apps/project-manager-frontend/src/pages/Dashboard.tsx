import { useEffect, useState } from "react"; import { api } from "../api"; import { Link, useNavigate } from "react-router-dom";

type Project={id:number; title:string; description?:string};
export default function Dashboard(){
  const nav = useNavigate();
  const [list,setList]=useState<Project[]>([]); const [title,setTitle]=useState(""); const [desc,setDesc]=useState("");
  useEffect(()=>{ api.get("/api/v1/projects").then(r=>setList(r.data)); },[]);
  async function add(){ const {data}=await api.post<Project>("/api/v1/projects", {title, description:desc}); setList(v=>[data,...v]); setTitle(""); setDesc(""); }
  function logout(){ localStorage.removeItem("token"); nav("/"); }
  return (
    <div className="p-4 max-w-4xl mx-auto space-y-4">
      <div className="nav p-3 flex items-center gap-2">
        <h1 className="text-2xl font-bold">Project Manager by Manav Gupta</h1>
        <button className="ml-auto btn" onClick={logout}>Logout</button>
      </div>
      <div className="card p-4 grid md:grid-cols-2 gap-2">
        <input className="input" placeholder="Project title" value={title} onChange={e=>setTitle(e.target.value)} />
        <input className="input" placeholder="Description (optional)" value={desc} onChange={e=>setDesc(e.target.value)} />
        <button className="btn md:col-span-2" onClick={add}>Create Project</button>
      </div>
      <div className="grid md:grid-cols-2 gap-3">
        {list.map(p=> (
          <Link key={p.id} to={`/p/${p.id}`} className="card p-4 hover:bg-white/10 transition">
            <div className="text-lg font-semibold">{p.title}</div>
            <div className="opacity-70 text-sm line-clamp-2">{p.description||""}</div>
          </Link>
        ))}
      </div>
    </div>
  );
}
