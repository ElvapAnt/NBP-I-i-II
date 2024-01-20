import { Link } from "react-router-dom"
import { CURRENT_USER } from "../../Constants"
import './LikeCard.css'
export function LikeCardLoader({ params })
{

}

export default function LikeCard({ props })
{
    const {thumbnail,username,userId} = props
    return <div className="like_card_container">
        <img src={thumbnail} className="like_thumbnail"></img>
        <Link className="like_username" to={'/Profile/'+props.userId}>{username}</Link>
    </div>
}