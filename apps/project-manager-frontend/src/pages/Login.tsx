import { useState } from "react"; import { api } from "../api"; import { useNavigate } from "react-router-dom";
export default function Login(){
  const nav = useNavigate();
  const [u,setU]=useState(""); const [p,setP]=useState(""); const [isReg,setReg]=useState(false); const [err,setErr]=useState("");
  async function submit(){
    try{
      const url = isReg? "/api/v1/auth/register" : "/api/v1/auth/login";
      const {data} = await api.post(url,{username:u,password:p});
      localStorage.setItem("token", data.token); nav("/dash");
    }catch{ setErr("Check credentials / server."); }
  }
  return (
    <div className="grid place-items-center min-h-screen">
      <div className="card p-6 w-[360px] space-y-3">
        <h1 className="text-xl font-bold">{isReg?"Create account":"Welcome back"}</h1>
        <input className="input" placeholder="Username" value={u} onChange={e=>setU(e.target.value)} />
        <input className="input" type="password" placeholder="Password" value={p} onChange={e=>setP(e.target.value)} />
        {err && <div className="text-red-300 text-sm">{err}</div>}
        <button className="btn w-full" onClick={submit}>{isReg?"Register":"Login"}</button>
        <button className="text-sm opacity-75" onClick={()=>setReg(!isReg)}>{isReg?"Have an account? Login":"New here? Register"}</button>
      </div>
    </div>
  );
}
