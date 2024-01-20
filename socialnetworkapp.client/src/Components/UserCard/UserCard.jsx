import { Link } from "react-router-dom"

export default function UserCard({ props })
{
    const { username, thumbnail, userId } = props
    
    return <div style={{
        width: "90%",
        height: "80px",
        padding:'10px'
    }}>
        <img src={thumbnail} style={{
            width:"60px",height:"60px"
        }} />
        <Link to={``}>{username}</Link>
    </div>
}